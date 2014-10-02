using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace IntegrationTestViewer
{

    public static class CustomResponseFormatters
    {
      public static Response AsStream<TModel>(this IResponseFormatter formatter, TModel stream, string contentType)
      {
        return new StreamResponse<TModel>(stream, contentType);
      }
    }

    public class StreamResponse<TModel> : Response
    {
      public StreamResponse(TModel model, string contentType)
      {
        this.Contents = GetStream(model);
        this.ContentType = contentType;
        this.StatusCode = HttpStatusCode.OK;
      }

      private static Action<Stream> GetStream(TModel model)
      {
        return stream =>
        {
          stream = model as Stream;
        };
      }
    }
  
}