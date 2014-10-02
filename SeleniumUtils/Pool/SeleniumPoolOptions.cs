using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneFin.SeleniumUtils.Pool
{
  public class SeleniumPoolOptions
  {

    /// <summary>
    /// The amount of sessions to keep running on startup
    /// </summary>
    public int? MinSessions { get; set; }

    /// <summary>
    /// The max amount of sessions to keep. This metric is usually limited by the 
    /// amount of tests you want to run simultaneously and the amount of memory and cpu available on the selenium server
    /// </summary>
    public int? MaxSessions { get; set; }

    /// <summary>
    /// Address of the selenium server. Don't forget to add the /wd/hub to the url
    /// </summary>
    public string SeleniumServerAddress { get; set; }

    public OpenQA.Selenium.Remote.DesiredCapabilities DesiredCapabilities { get; set; }
  }
}
