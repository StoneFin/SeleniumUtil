using StoneFin.SeleniumUtils.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntegrationTestViewer.Models
{
  public interface IRepository
  {
    IList<ITestBatch> GetBatchList();
    IList<ITestResult> GetResultsForBatch(string BatchID);

    ITestResult GetTestResult(string trid);
  }
}