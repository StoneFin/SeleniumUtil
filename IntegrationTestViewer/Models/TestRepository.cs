using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using StoneFin.SeleniumUtils.Test;
using MongoDB.Bson.Serialization;

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
    private MongoCollection<T> GetCollection<T>(string colName = "digiwidgets")
    {
      //my first app with mongo! (*)_(
      var testCol = this._db.GetCollection<T>(colName);
      return testCol;
    }
    public PagedResult<ITestBatch> GetBatchList(PagedResultArgs args = null)
    {
      if (args == null)
      {
        args = new PagedResultArgs(0, 30);
      }

      //mongo aggregation is a pipeline. Each operation is performed on (and modifies) the currently matched set, in order.
      var aggregateArgArray = new BsonDocument[] {
        //sort by insertion, so the $first below works
        BsonDocument.Parse(@"{$sort:{_id:1}}"), 
        //group by the batch ID, calculate the total and passed tests, get the batchdate from the first result in the set.
        //note: that batchdate might not be the exact time the batch started, it's the time the first completed test was 
        //succesfully inserted, so there's a race condition there w/ unit testing. It would be more accurate to get 
        //the earliest date from the grouped elements
        BsonDocument.Parse(@"{$group:{_id:'$BatchID',TotalTests:{$sum:1},'PassedTests':{$sum:{$cond:[{$eq:['$NavigationSuccessful',true]},1,0]}},
                BatchDate:{$first:'$DateRan'}}}"),
        //now project the grouping, this is the "Select" part of the query. Exclude the _id field.
        BsonDocument.Parse("{$project:{_id:false,BatchID:'$_id',TotalTests:'$TotalTests',TotalPasses:'$PassedTests',BatchDate:'$BatchDate'}}"), 
        //now sort the batches by their date - b/c of the race condition and the lazy query the batches might be out of order, even though we 
        //sorted by ID when grouping!
        BsonDocument.Parse(@"{$sort:{BatchDate:-1}}"),
        BsonDocument.Parse("{$skip:" + args.Index * args.ResultsPerPage + "}"),
        BsonDocument.Parse("{$limit:" + args.ResultsPerPage +"}")                                                                            
      };
      //limit to the most recent 50 groupings. Have to do a javascript grouping b/c linq driver doesn't support group yet.
      var batchGroupings = GetCollection<TestResult>().Aggregate(aggregateArgArray).ResultDocuments;
      var theBatches = batchGroupings.Select(x => { try { return (ITestBatch)BsonSerializer.Deserialize<TestBatch>(x); } catch (Exception ex) { return new TestBatch() { BatchID = ex.Message.ToString() }; } }).Where(x => x != null).ToList();
      //now get the count and return the pagedResults;
      //aggregate pipeline with two groupings. Reduce the collection to distinct batch IDs, then reduce that to a count
      var theCount = GetCollection<TestResult>().Aggregate(
        BsonDocument.Parse(@"{$group:{_id:'$BatchID'}}"),
        BsonDocument.Parse(@"{$group:{_id:1,count:{$sum:1}}}"));
      int totalCount = theCount.ResultDocuments.First().GetElement("count").Value.AsInt32;
      return new PagedResult<ITestBatch>(theBatches, totalCount, args);
      //return result.ResultDocuments.Select(x=>x.Get)
      //  .Select(x=>new TestBatch() {
      //    TotalTests = x.Count(),
      //    TotalPasses=x.Where(y=>y.NavigationSuccessful == true).Count(),
      //    BatchID = x._,
      //    BatchDate=x.First().DateRan}).Cast<ITestBatch>().ToList();
    }

    public IList<ITestResult> GetResultsForBatch(string BatchID)
    {
      var query = Query.EQ("BatchID", new BsonString(BatchID));
      return GetCollection<TestResult>().Find(query).Cast<ITestResult>().ToList();
    }
    public ITestResult GetTestResult(string trid)
    {
      var query = Query.EQ("_id", ObjectId.Parse(trid));
      return GetCollection<TestResult>().FindOne(query);
    }
  }
  public class PagedResult<T> : IEnumerable<T>  {
      private  IEnumerable<T> _source;
      public readonly int TotalMatches;
      public PagedResultArgs PageArgs { get; private set; }
      public PagedResult(IEnumerable<T> source,int totalMatches,PagedResultArgs args) {
        this._source = source;
        this.PageArgs = args;
        this.TotalMatches = totalMatches;
      }

      public IEnumerator<T> GetEnumerator()
      {
        foreach (var s in _source)
          yield return s;
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _source.GetEnumerator();
      }
  }
  public class PagedResultArgs {
    public PagedResultArgs(int Index, int ResultsPerPage) {
      this.Index = Index;
      this.ResultsPerPage = ResultsPerPage;
    }
    public readonly int Index;
    public readonly int ResultsPerPage;
  }
}