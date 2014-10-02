using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoneFin.SeleniumUtils
{
  public static class WebDriverExtensions
  {
    public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
    {
      if (timeoutInSeconds > 0)
      {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
        return wait.Until(drv => drv.FindElement(by));
      }
      return driver.FindElement(by);
    }
    public static void WaitUntilScript(this RemoteWebDriver driver, string ScriptToExecute, int timeoutInSeconds = 20)
    {
      var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
      wait.Until(drv => Boolean.Parse(driver.ExecuteScript(ScriptToExecute).ToString()) == true);
    }
  }
}
