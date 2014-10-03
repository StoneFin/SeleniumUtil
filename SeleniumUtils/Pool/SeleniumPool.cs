using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenQA.Selenium.Firefox;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Remote;

namespace StoneFin.SeleniumUtils.Pool
{
  public class SeleniumPool : IDisposable
  {
    private int MINSESSIONS;
    private int MAXSESSIONS;
    private string _address;
    private List<IntDisposableBrowser> _browsers;
    private DesiredCapabilities _desiredCapabilities;
    internal SeleniumPool(SeleniumPoolOptions options)
    {
      this._address = options.SeleniumServerAddress;
      this.MINSESSIONS = options.MinSessions.Value;
      this.MAXSESSIONS = options.MaxSessions.Value;
      if (this._address == null)
        throw new ArgumentException("SeleniumServerAddress");
      if (this.MINSESSIONS == null)
        throw new ArgumentException("MinSessions");
      if (this.MAXSESSIONS == null)
        throw new ArgumentException("MaxSessions");
      this._browsers = new List<IntDisposableBrowser>(this.MAXSESSIONS);
      this._desiredCapabilities = options.DesiredCapabilities ?? DesiredCapabilities.Firefox();
      init();
    }
    private void init()
    {
      //no need to spin up browsers immediately for debug.
#if !DEBUG
      for (int i = 0; i < MINSESSIONS; i++)
      {
        var inst = getNewSelenium();
        _browsers.Add(new IntDisposableBrowser(inst));
      }
#endif
    }

    public DisposableBrowser Aquire()
    {
      var first = _browsers.FirstOrDefault(x => x.InUse == false);
      if (first != null)
      {
        //so it can't get snagged up.
        //really, this whole section should probably lock on the list...
        first.InUse = true;
        //ping it, if it throws an exception recreate it.
        try
        {
          first.Browser.Browser.ExecuteScript(@"return window.location.toString()");
        }
        catch (InvalidOperationException exc)
        {
          try
          {
            _browsers.Remove(first);
            first.Browser.Destroy();
          }
          catch {
          //sorry not sorry. If the remote session has crashed all of these will throw exceptions, 
          //but we'll probably still be able to 
          //create a new one.
          //So, swallow the exceptions.
          }
          first = null;
        }
      }

      if (first == null)
      {
        if (_browsers.Count > MAXSESSIONS)
        {
          throw new InvalidOperationException("All renderer sessions in use");
        }
        //they were already in use, add another one.
        first = new IntDisposableBrowser(getNewSelenium(this._desiredCapabilities));
        //mark it as in use before we add it to the list!
        first.InUse = true;
        _browsers.Add(first);
      }
      return first.Browser;
    }
    private RemoteWebDriver getNewSelenium(DesiredCapabilities desiredCapabilities)
    {
      var browser = new RemoteWebDriver(new Uri(this._address), desiredCapabilities);
      return browser;
    }

    public void Dispose()
    {
      foreach (var b in this._browsers)
        try
        {
          b.Dispose();
        } catch { }
    }
  }
}