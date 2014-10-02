using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneFin.SeleniumUtils.Test
{
  public class TestResult : ITestResult
  {
    public string ActualURL { get; set; }
    public string BatchID { get; set; }
    public DateTime DateRan { get; set; }
    public string ExpectedURL { get; set; }
    public string FullyQualifiedTestClassName { get; set; }
    public bool? NavigationSuccessful { get; set; }
    public string PageSource { get; set; }
    public byte[] ScreenShot { get; set; }
    public string TestName { get; set; }
    public int TestResults { get; set; }
    public string TestResultStr { get; set; }
    public TimeSpan Timing { get; set; }
    public string UriTested { get; set; }
  }
}
