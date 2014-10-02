using System;
namespace StoneFin.SeleniumUtils.Test
{
  public interface ITestBatch
  {
    string ProjectName { get; set; }
    DateTime BatchDate { get; set; }
    string BatchID { get; set; }
    int TotalFailures { get; }
    int TotalPasses { get; set; }
    int TotalTests { get; set; }
  }
}
