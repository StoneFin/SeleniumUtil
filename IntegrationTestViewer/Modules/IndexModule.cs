using IntegrationTestViewer.Models;
using Nancy;
using System;
using System.Linq;

namespace IntegrationTestViewer.Modules
{
  public class IndexModule : NancyModule
  {
    private IRepository _repo;
    public IndexModule(IRepository repo)
    {
      this._repo = repo;
      Get["/"] = frontPage;
      Get["/page/{Page}"] = frontPage;
      Get["/page/{Page}/results/{PageSize}"] = frontPage;
      Get["/batch/{batchid}"] = parameters =>
      {
        string batchID = parameters.batchid;
        var theViewData = _repo.GetResultsForBatch(batchID).Cast<TestResult>().ToList();
        return View["batchdetails", theViewData];
      };
      Get["/screenshots/{trid}"] = parameters =>
      {
        string trid = parameters.trid;
        var ms = new System.IO.MemoryStream(_repo.GetTestResult(trid).ScreenShot);
        return Response.FromStream(ms, "image/png");
      };
    }

    private dynamic frontPage(dynamic parameters)
    {
      int defaultPage = 1;
      int defaultSize = 50;
      if (parameters.Page != null)
      {
        defaultPage = Math.Max(1, int.Parse(parameters.Page));
      }
      if (parameters.PageSize != null)
      {
        defaultSize = Math.Max(15, int.Parse(parameters.PageSize));
      }
      var resultArgs = new PagedResultArgs(defaultPage - 1, defaultSize);
      var theViewData = _repo.GetBatchList(resultArgs);
      return View["batchindex", theViewData];
    }
  }
}