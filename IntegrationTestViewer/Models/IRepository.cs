using StoneFin.SeleniumUtils.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntegrationTestViewer.Models
{
  public interface IRepository
  {
    PagedResult<ITestBatch> GetBatchList(PagedResultArgs args);
    IList<ITestResult> GetResultsForBatch(string BatchID);

    ITestResult GetTestResult(string trid);
  }
}