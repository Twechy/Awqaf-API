//#if NETCOREAPP3_1

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using MittDevQA.Utils.Vm;
//using Newtonsoft.Json;
//using Serilog;

//namespace MittDevQA.Utils.Logging
//{
//    public class SerilogRequestLoggerMiddleware
//    {
//        private readonly Func<Exception, string> _executeWhenErrorHappen;
//        private readonly List<string> _excludeFromLogResponse;
//        private readonly RequestDelegate _next;
//        private HttpTraceModel _traceModel;

//        public SerilogRequestLoggerMiddleware(RequestDelegate next,
//            Func<Exception, string> executeWhenErrorHappen, List<string> excludeFromLogResponse = null)
//        {
//            if (next == null) throw new ArgumentNullException(nameof(next));
//            _next = next;
//            _executeWhenErrorHappen = executeWhenErrorHappen;
//            _excludeFromLogResponse = excludeFromLogResponse;
//        }

//          //var bodyStr = "";
//          //  var req = context.Request;
//          //  req.EnableBuffering();

//          //  using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
//          //  {
//          //      bodyStr = await reader.ReadToEndAsync();
//          //      req.Body.Position = 0;
//          //  }

//          //  await next(context);

//        public async Task Invoke(HttpContext httpContext)
//        {
//            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

//            // Getting the request body is a little tricky because it's a stream
//            // So, we need to read the stream and then rewind it back to the beginning

//            var requestBody = "";
//            httpContext.Request.EnableBuffering();

//            var body = httpContext.Request.Body;
//            var buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
//            await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);

//            requestBody = Encoding.UTF8.GetString(buffer);
//            body.Seek(0, SeekOrigin.Begin);
//            httpContext.Request.Body = body;
//            var service = httpContext.RequestServices.GetService<IHttpContextAccessor>();
//            _traceModel = new HttpTraceModel(service);

//            // The reponse body is also a stream so we need to:
//            // - hold a reference to the original response body stream
//            // - re-point the response body to a new memory stream
//            // - read the response body after the request is handled into our memory stream
//            // - copy the response in the memory stream out to the original response stream
//            using (var responseBodyMemoryStream = new MemoryStream())
//            {
//                var originalResponseBodyReference = httpContext.Response.Body;
//                httpContext.Response.Body = responseBodyMemoryStream;
//                Exception currentExp = null;
//                try
//                {
//                    await _next(httpContext);
//                }

//                catch (Exception exception)
//                {
//                    currentExp = exception;
//                    var result = _executeWhenErrorHappen(exception);
//                    httpContext.Response.Clear();

//                    httpContext.Response.ContentType = "application/json";
//                    httpContext.Response.StatusCode = 200;
//                    await httpContext.Response.WriteAsync(result);

//                }
//                var requestBodyResult = logRequest(httpContext);

//                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
//                var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
//                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
//                if (currentExp != null)
//                {
//                    Log.Logger.ForContext("Type", "Error")
//                                       .ForContext("RequestHeaders",
//                                           httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
//                                           true).ForContext("RequestBody", requestBodyResult, true).ForContext("TraceModel", _traceModel, true).ForContext("ResponseBody",
//                                             deserializeObj(responseBody), true)
//                                       .Error(currentExp, "Response  {RequestMethod} {RequestPath} {statusCode} ",
//                                       httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode);

//                }
//                else
//                {
//                    Log.Logger.ForContext("RequestHeaders",
//                                         httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
//                                         true).ForContext("RequestBody", requestBodyResult, true).ForContext("TraceModel", _traceModel, true)
//                                     .ForContext("ResponseBody",
//                                         httpContext.Response.ContentType?.Replace(" ", "") == "application/json;charset=utf-8"
//                                             ? deserializeObj(responseBody)
//                                             : httpContext.Response.ContentType, true)
//                                     .Information("Response information {RequestMethod} {RequestPath} {statusCode} ",
//                                         httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode
//                                     );

//                }

//                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference);
//            }
//        }

//        private static object logRequest(HttpContext httpContext)
//        =>
//           (Dictionary<string, object>)httpContext.Items["Args"];

//        private object deserializeObj(string responseBody)
//        {
//            if (_excludeFromLogResponse != null && !string.IsNullOrEmpty(_traceModel.ApiId)
//                && _excludeFromLogResponse.Contains(_traceModel.ApiId))
//            {
//                var opObj = JsonConvert.DeserializeObject<OperationResultLog>(responseBody);
//                return opObj;

//            }

//            return JsonConvert.DeserializeObject(responseBody); ;

//        }
//    }

//    public class FormField
//    {
//        public string FieldKey { get; set; }

//        public string FieldValue { get; set; }
//    }

//    public class OperationResultLog
//    {
//        public int type { get; set; }

//        public string[] messages { get; set; }

//    }
//}

//#endif