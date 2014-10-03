SeleniumUtil
============
____________
##SeleniumUtils

Helpful classes for running integration tests with selenium, mongodb and visual studio test framework

Two major classes, SeleniumPool and SeleniumTestBase

#####Example usage

(with bonus Moq && controller integration! It')

```csharp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StoneFin.SeleniumUtils.Pool;
using StoneFin.SeleniumUtils.Test;

 [TestClass]
  public class SeleniumWarmup : SeleniumTestBase
  {
    public SeleniumWarmup()
    {
      this.BaseURL = "http://myapp.com";
      //this needs to match the test user account!!
      //used to generate the cookie for authentication.
      this.LoginInfo = new MyApp.Membership.MembershipInfo() { 
        RememberMe = true, UserID = 3, UserName = "gabe@stonefintech.com" 
      };
      
      var context = setupContextForControllers();
      context.Request.SetupRequestUrl("~/");
    }

    #region TestStuff
    [ClassInitialize]
    public static void ClassInitialize(TestContext ctx)
    {
      //this needs to match the test user account!!
      var loggingOptions = new LogRepoOptions() { 
        collectionName = "MyApp", 
        connectionString = "mongodb://mongoserver.local", 
        dbName = "seleniumTests" 
      };
      var poolOptions = new SeleniumPoolOptions()
      {
        MaxSessions = 10,
        MinSessions = 1,
        SeleniumServerAddress = "http://stonefindev.hofstadter.iso:4444/wd/hub"
      }; 
      SeleniumTestBase.ClassInitialize(poolOptions,loggingOptions);
    }


    [TestInitialize]
    public override void TestInitialize()
    {
      base.TestInitialize();
      this.VerifyLoggedOut();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
      //i'm not sure, but I don't think exceptions here will affect the pass/fail of the test run.
      //if we could somehow ensure that allcontrollersaretested would be a test 
      //that runs at the end that would be aweomse, but otherwise no
      try
      {
        EnsureAllControllersAreTested();
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        SeleniumTestBase.ClassCleanup();
      }
 	    
    }

    [TestCleanup]
    public override void TestCleanup()
    {
      base.TestCleanup();
    }

    public static void EnsureAllControllersAreTested()
    {
      Assembly a = typeof(MyApp.Controllers.HomeController).Assembly;
      var list = a.GetTypes().Where(type => typeof(System.Web.Mvc.IController).IsAssignableFrom(type)).ToList();
      var testedURLs = TestedURLS.Select(x=>new Uri(x).PathAndQuery.TrimStart('/')).ToList();
      List<String> failures = new List<String>();
      foreach (var controller in list)
      {
        bool home = controller.Name == "HomeController";
        bool overrideHome = false;
        if (home)
        {
          overrideHome = testedURLs.Any(x => x == "");
        }

        if (!testedURLs.Any(x => x.StartsWith(controller.Name.Replace("Controller",""))) || (home && !overrideHome))
          failures.Add(controller.Name);
      }
      if (failures.Any())
        Assert.Fail("Some controllers weren't tested! " + String.Join(",", failures.ToArray()));
    }

    #endregion
    #region Tests

    [TestMethod]
    public void TestHomeRedirect()
    {
      NavigateAndVerify(UrlHelper.Action("Index", "Home"), "/");
      this.VerifyLoggedIn();
      this.Browser.Navigate().Refresh();
      this.LogURL(Browser.Url);
      NavigateAndVerify(UrlHelper.Action("Index", "Home"),"/Design");
    }


    [TestMethod]
    public void TestUnAuth()
    {
      TestLogin();
      Assert.IsTrue(VerifyLoggedIn(), "not logged in");
      Browser.FindElementById("logoutForm").Submit();
      VerifyURL(UrlHelper.Action("Login","Account"));
      Assert.IsTrue(this.VerifyLoggedOut(),"Logout failed");
    }

    [TestMethod]
    public void Test500()
    {
      NavigateAndVerify(UrlHelper.Action("ThrowException","Account"));
      //it should be a slient redirect to the login page with a message when not logged in.
      var theMessage = Browser.FindElementByCssSelector(@"#top > div.alert.alert-info");
      var userLogin = Browser.FindElementById("UserName");
      Assert.IsNotNull(userLogin, "the login element wasn't there");
      Assert.IsNotNull(theMessage,"the message wasn't found");
      Assert.AreEqual(theMessage.Text, "You must login to view that.");
      VerifyLoggedIn();
      Browser.Navigate().Refresh();
      VerifyURL(UrlHelper.Action("ThrowException", "Account"));
      testErrorPage(500);
    }
    [TestMethod]
    public void Test404()
    {
      NavigateAndVerify(UrlHelper.Action("asdfasdfasdf", "Account"));
      testErrorPage(404);
    }

    private void testErrorPage(int statusCode) {
      Assert.AreEqual(Browser.FindElementById("ErrorNumber").GetAttribute("value"),statusCode.ToString());
    }
    [TestMethod]
    public void TestLogin()
    {
      NavigateAndVerify(UrlHelper.Action("Login","Account"));
      Browser.FindElementById("UserName").SendKeys("gabe@stonefintech.com");
      var pw = Browser.FindElementById("Password");
      pw.SendKeys("password");
      pw.Submit();
      VerifyURL(UrlHelper.Action("Index","Design"));
    }
    #endregion

    #region Helpers
    private System.Web.Mvc.UrlHelper _helper;
    public System.Web.Mvc.UrlHelper UrlHelper { get { return _helper; } }
    public HttpContextBase setupContextForControllers()
    {
      var context = new Mock<HttpContextBase>();
      var request = new Mock<HttpRequestBase>();
      var response = new Mock<HttpResponseBase>();
      var session = new Mock<HttpSessionStateBase>();
      var server = new Mock<HttpServerUtilityBase>();

      context.Setup(ctx => ctx.Request).Returns(request.Object);
      context.Setup(ctx => ctx.Response).Returns(response.Object);
      context.Setup(ctx => ctx.Session).Returns(session.Object);
      context.Setup(ctx => ctx.Server).Returns(server.Object);

      request.SetupGet(x => x.ApplicationPath).Returns("/");
      request.SetupGet(x => x.Url).Returns(new Uri(this.BaseURL, UriKind.Absolute));
      request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());

      response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => x);

      context.SetupGet(x => x.Request).Returns(request.Object);
      context.SetupGet(x => x.Response).Returns(response.Object);


      var routes = new RouteCollection();
      RouteConfig.RegisterRoutes(routes);
      _helper = new UrlHelper(new RequestContext(context.Object, new RouteData()), routes);
      return context.Object;

    }
    protected bool VerifyLoggedOut()
    {
      if (Browser.Manage().Cookies.GetCookieNamed("MyApp*)efdaew$%s") == null)
        return true;
      this.ClearCookies();
      return false;
    }
    protected bool VerifyLoggedIn()
    {
      if (Browser.Manage().Cookies.GetCookieNamed("MyApp*)efdaew$%s") == null)
      {
        var cookieString = MyApp.Membership.MembershipHelper.PrepareMembershipInfo(_cookieInfo);
        Browser.Manage().Cookies.AddCookie(
          new OpenQA.Selenium.Cookie(MyApp.Membership.MembershipHelper.GetAuthCookieName(), cookieString));
        return false;
      }
      return true;
    }

    private MyApp.Membership.MembershipInfo _cookieInfo;

    public MyApp.Membership.MembershipInfo LoginInfo
    {
      get { return _cookieInfo; }
      set { _cookieInfo = value; }
    }
    #endregion
  }


```


##IntegrationTestViewer
A web project with Nancy Razor and TinyIOC to view stored results of test runs, including page source and screenshots.
