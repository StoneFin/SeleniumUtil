using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneFin.SeleniumUtils.Pool
{

  internal sealed class IntDisposableBrowser : IDisposable
  {
    internal IntDisposableBrowser(RemoteWebDriver browser)
    {
      this.Browser = new DisposableBrowser(browser, _ => this.InUse = false);
    }
    public bool InUse = false;
    public DisposableBrowser Browser;

    public void Dispose()
    {
      this.Browser.Destroy();
    }
  }

  /// <summary>
  ///    A browser instance is marked in use by the pool when it's Aquired. 
  ///    Dispose must be called to make the instance available to the pool again!
  /// </summary>
  public class DisposableBrowser : IDisposable
  {
    internal DisposableBrowser(RemoteWebDriver browser, Action<bool> onDispose)
    {
      this.Browser = browser;
      this._onDispose = onDispose;
    }
    private Action<bool> _onDispose;
    public RemoteWebDriver Browser;

    /// <summary>
    /// returns the browser back to the pool in an available state after deleting all cookies and navigating to about:blank;
    /// </summary>
    public void Dispose()
    {
      this.Browser.Manage().Cookies.DeleteAllCookies();
      this.Browser.Navigate().GoToUrl("about:blank");
      this._onDispose(true);
    }
    /// <summary>
    /// disposes and kills the selenium browser instance instead of just returning the instance back to the pool
    /// </summary>
    public void Destroy()
    {
      this.Browser.Dispose();
      this._onDispose(true);
    }
  }
}
