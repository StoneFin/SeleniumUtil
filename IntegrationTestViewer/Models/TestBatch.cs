using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoneFin.SeleniumUtils.Test;

namespace IntegrationTestViewer.Models
{
  public class TestBatch : ITestBatch
  {
    public string ProjectName { get; set; }
    public string BatchID { get; set; }
    public DateTime BatchDate { get; set; }
    public int TotalTests { get; set; }
    public int TotalPasses { get; set; }
    public int TotalFailures { get { return TotalTests - TotalPasses; } }
  }
}
