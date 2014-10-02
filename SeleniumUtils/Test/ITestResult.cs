using System;

namespace StoneFin.SeleniumUtils.Test
{
  public interface ITestResult
  {
    string ActualURL { get; set; }
    string BatchID { get; set; }
    DateTime DateRan { get; set; }
    string ExpectedURL { get; set; }
    string FullyQualifiedTestClassName { get; set; }
    bool? NavigationSuccessful { get; set; }
    string PageSource { get; set; }
    byte[] ScreenShot { get; set; }
    string TestName { get; set; }
    int TestResults { get; set; }
    string TestResultStr { get; }
    TimeSpan Timing { get; set; }
    string UriTested { get; set; }
  }
}
