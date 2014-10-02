using IntegrationTestViewer.Models;
using Nancy;
using System.Linq;

namespace IntegrationTestViewer.Modules
{
  public class IndexModule : NancyModule
  {
    private IRepository _repo;
    public IndexModule(IRepository repo)
    {
      this._repo = repo;
      Get["/"] = parameters =>
      {
        var theViewData = _repo.GetBatchList().OrderByDescending(x=>x.BatchDate).Cast<TestBatch>().ToList();
        return View["batchindex", theViewData];
      };
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
  }
}