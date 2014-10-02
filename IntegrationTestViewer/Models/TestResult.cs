using StoneFin.SeleniumUtils.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTestViewer.Models
{
  public class TestResult : ITestResult
  {
    public TestResult(string name) { this.TestName = name; }
    public TestResult() { }
    public MongoDB.Bson.ObjectId Id { get; set; }
    public string UriTested { get; set; }
    public string TestName { get; set; }
    public string BatchID { get; set; }
    public bool? NavigationSuccessful { get; set; }

    public string ExpectedURL { get; set; }
    public string ActualURL { get; set; }
    public int TestResults { get; set; }
    public string TestResultStr { get; set; }
    public TimeSpan Timing { get; set; }
    public byte[] ScreenShot { get; set; }
    public string PageSource { get; set; }
    public string FullyQualifiedTestClassName { get; set; }
    public DateTime DateRan { get; set; }
  }
}
