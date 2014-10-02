using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using StoneFin.SeleniumUtils.Test;

namespace IntegrationTestViewer.Models
{
  public class TestRepository : IRepository
  {
    private MongoDatabase _db;
    public TestRepository()
    {
      var connectionString = System.Web.Configuration.WebConfigurationManager.AppSettings["mongoconnectionstring"];
      var client = new MongoClient(connectionString);
      var server = client.GetServer();
      var dbName = System.Web.Configuration.WebConfigurationManager.AppSettings["mongodbname"];
      this._db = server.GetDatabase(dbName);
    }
    //todo refactor so colname is a parameter in public methods based on ProjectName so we can use this for multiple projects!
    private MongoCollection<T> GetCollection<T>(string colName = "digiwidgets") {
      //my first app with mongo! (*)_(
      var testCol = this._db.GetCollection<T>(colName);
      return testCol;
    }
    public IList<ITestBatch> GetBatchList()
    {
      return GetCollection<TestResult>().FindAll().ToList()
        .GroupBy(x=>x.BatchID).ToList()
        .Select(x=>new TestBatch() {
          TotalTests = x.Count(),
          TotalPasses=x.Where(y=>y.NavigationSuccessful == true).Count(),
          BatchID = x.Key,
          BatchDate=x.First().DateRan}).Cast<ITestBatch>().ToList();
    }

    public IList<ITestResult> GetResultsForBatch(string BatchID)
    {
      var query = Query.EQ("BatchID",new BsonString(BatchID));
      return GetCollection<TestResult>().Find(query).Cast<ITestResult>().ToList();
    }
    public ITestResult GetTestResult(string trid)
    {
      var query = Query.EQ("_id", ObjectId.Parse(trid));
      return GetCollection<TestResult>().FindOne(query);
    }
  }
}