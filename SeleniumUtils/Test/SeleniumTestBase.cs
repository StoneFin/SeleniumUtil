using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using StoneFin.SeleniumUtils.Pool;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StoneFin.SeleniumUtils.Test
{
  public abstract class SeleniumTestBase
  {
    private TestContext testContextInstance;
    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    protected static List<TestResult> TestInfos = new List<TestResult>();
    protected static List<string> TestedURLS = new List<string>();

    protected OpenQA.Selenium.Remote.RemoteWebDriver Browser { get { return _browser.Browser; } }
    private DisposableBrowser _browser;

    protected static string BatchID = "";

    /// <summary>
    /// Sets the BatchID to the first 10 characters of a guid.
    /// </summary>
    public static void ClassInitialize(SeleniumPoolOptions poolOptions,LogRepoOptions repoOptions)
    {
      RepoOptions = repoOptions;
      SeleniumPoolInst = SeleniumPoolFactory.GetPool(poolOptions);
      canDisposeOfPool = true;
      BatchID = Guid.NewGuid().ToString("n").Substring(0, 10);
    }

    /// <summary>
    /// Sets the BatchID to the first 10 characters of a guid.
    /// </summary>
    public static void ClassInitialize(SeleniumPool pool, LogRepoOptions repoOptions)
    {
      RepoOptions = repoOptions;
      SeleniumPoolInst = pool;
      canDisposeOfPool = false;
      BatchID = Guid.NewGuid().ToString("n").Substring(0, 10);
    }

    /// <summary>
    /// If possible, disposes of the seleniumPool, freeing up resources on the selenium server.
    /// </summary>
    public static void ClassCleanup()
    {
      if (canDisposeOfPool)
        SeleniumPoolInst.Dispose();
    }


    private string _baseURL;

    /// <summary>
    /// Used for logging and navigation.
    /// </summary>
    public string BaseURL
    {
      get { return _baseURL; }
      set { _baseURL = value; }
    }

    private System.Diagnostics.Stopwatch _stopwatch;
    protected static LogRepoOptions RepoOptions;
    protected static SeleniumPool SeleniumPoolInst;
    private static bool canDisposeOfPool = true;

    public virtual void TestInitialize()
    {
      _stopwatch = new System.Diagnostics.Stopwatch();
      _browser = SeleniumPoolInst.Aquire();
      TestInfos.Clear();
      _stopwatch.Reset();
      _stopwatch.Start();
    }
    public virtual void TestCleanup()
    {
      _stopwatch.Stop();
      try
      {
        SaveTestInfo(_stopwatch.Elapsed);
      }
      catch (Exception ex)
      {

        throw;
      }
      this._browser.Dispose();
      TestedURLS.AddRange(TestInfos.Select(x => x.UriTested));
      TestInfos.Clear();
    }

    private void SaveTestInfo(TimeSpan timeSpan)
    {
      var client = new MongoClient(RepoOptions.connectionString);
      var server = client.GetServer();
      var testDB = server.GetDatabase(RepoOptions.dbName);
      var testCol = testDB.GetCollection(RepoOptions.collectionName);
      var theResults = TestInfos.ToList();
      var dateRan = DateTime.Now;
      foreach (var tr in theResults)
      {
        tr.TestResults = (int)TestContext.CurrentTestOutcome;
        tr.TestResultStr = TestContext.CurrentTestOutcome.ToString();
        tr.FullyQualifiedTestClassName = TestContext.FullyQualifiedTestClassName;
        tr.DateRan = dateRan;
      }
      testCol.InsertBatch<TestResult>(theResults);
    }



    protected TestResult l(string URL)
    {
      var tr = new TestResult()
      {
        TestName = TestContext.TestName,
        UriTested = URL,
        BatchID = BatchID
      };
      TestInfos.Add(tr);
      return tr;
    }
    protected void LogURL(string url)
    {
      if (url == null)
        throw new ArgumentException("url");
      if (url.StartsWith("/") || url == "") ;
      url = _baseURL + url;
      string expectedURL = url;
      var tr = l(url);
      tr.ScreenShot = this.Browser.GetScreenshot().AsByteArray;
      tr.PageSource = this.Browser.PageSource;
      tr.NavigationSuccessful = true;
    }
    /// <summary>
    /// Logs and Verifies the browsers current url
    /// </summary>
    protected void VerifyURL()
    {
      var u = safeGetBrowserURL();
      VerifyURL(u);
    }
    /// <summary>
    /// Logs and verifies <paramref name="url"/>
    /// </summary>
    /// <param name="url">The url to log and verify the browser is currently at</param>
    protected void VerifyURL(string url)
    {
      if (url == null)
        throw new ArgumentException("url");
      //super janky way to support relative or full urls!
      if (url == "")
        url = "/";
      if (url.StartsWith("/"))
        url = _baseURL + url;
      string expectedURL = url;
      var tr = l(url);
      tr.ExpectedURL = expectedURL;
      tr.ScreenShot = this.Browser.GetScreenshot().AsByteArray;
      tr.PageSource = this.Browser.PageSource;
      try
      {
        tr.ActualURL = Browser.Url.TrimEnd('/');
        Assert.AreEqual(expectedURL.TrimEnd('/'), tr.ActualURL);
      }
      catch (Exception)
      {
        tr.NavigationSuccessful = false;
        throw;
      }
      tr.NavigationSuccessful = true;
    }

    /// <summary>
    /// Navigates and Verifies the given <paramref name="url"/> against the <paramref name="expectedURL"/>
    /// </summary>
    /// <param name="url">URL to navigate to</param>
    /// <param name="expectedURL">The expected destination url, for server redirects. Defaults to <paramref name="url" /></param>
    protected void NavigateAndVerify(string url, string expectedURL = null)
    {
      if (url == null)
        throw new ArgumentException("url");
      if (url.StartsWith("/") || url == "") ;
      url = _baseURL + url;

      if (expectedURL == null)
        expectedURL = url;
      else
        if (expectedURL.StartsWith("/") || expectedURL == "")
          expectedURL = _baseURL + expectedURL;

      var tr = l(url);
      tr.ExpectedURL = expectedURL;
      var sw = new System.Diagnostics.Stopwatch();
      sw.Start();
      Browser.Navigate().GoToUrl(url);
      sw.Stop();
      tr.Timing = sw.Elapsed;
      safeGetBrowserURL();
      tr.ScreenShot = this.Browser.GetScreenshot().AsByteArray;
      tr.PageSource = this.Browser.PageSource;
      try
      {
        tr.ActualURL = Browser.Url.TrimEnd('/');
        Assert.AreEqual(expectedURL.TrimEnd('/'), tr.ActualURL);
      }
      catch (Exception)
      {
        tr.NavigationSuccessful = false;
        throw;
      }
      tr.NavigationSuccessful = true;
    }


    /// <summary>
    /// There HAS to be a better way to do this! Basically sleep if selenium crashes while retrieving the browsers current URL.
    /// Maybe we could somehow inject a $(function() { window.NavigationCompleteSelenium = true }) and a WaitForJavascript driver extension?
    /// </summary>
    private string safeGetBrowserURL()
    {
      string a = null;
      try
      {
        a = Browser.Url;
      }
      //because this: https://code.google.com/p/selenium/issues/detail?id=3544
      //it's a timing thing with slower computers, aka stonefindev. 
      // So, just wait a tick for the browser to catch up.
      catch (System.InvalidOperationException ex)
      {
        System.Diagnostics.Debug.WriteLine("Exception Caught while getting browser url. Sleeping");
        System.Diagnostics.Debug.WriteLine(ex.ToString());
        /* 
         * Test method IntegrationTests.SeleniumWarmup.TestHomeRedirect threw exception: 
         * System.InvalidOperationException: [JavaScript Error: "e is null" {file: "
         */
        try
        {
          System.Threading.Thread.Sleep(200);
          a = Browser.Url;
        }
        catch
        {
          try
          {
            System.Diagnostics.Debug.WriteLine("Exception 2 Caught while getting browser url. Sleeping");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            System.Threading.Thread.Sleep(200);
            a = Browser.Url;
          }
          catch
          {
            System.Diagnostics.Debug.WriteLine("Exception 3 Caught while getting browser url. Sleeping");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            //no reason to throw - we'll try the getScreenshot later and if the browser is happy 
            //it'll work and if not it'll crash on the next line not matter what.
          }
        }
      }
      return a;
    }

    protected void ClearCookies()
    {
      Browser.Manage().Cookies.DeleteAllCookies();
    }

  }
}
