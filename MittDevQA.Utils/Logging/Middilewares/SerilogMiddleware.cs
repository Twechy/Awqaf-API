using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using Newtonsoft.Json;
using Serilog;
using Utils.Logging.Extensions;
using Utils.Vm;

namespace Utils.Logging.Middilewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly Func<Exception, string> _executeWhenErrorHappen;
        private readonly List<string> _excludeFromLogResponse;

        public RequestResponseLoggingMiddleware(RequestDelegate next, Func<Exception, string> executeWhenErrorHappen, List<string> excludeFromLogResponse = null)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _executeWhenErrorHappen = executeWhenErrorHappen;
            _excludeFromLogResponse = excludeFromLogResponse;
        }

        public async Task Invoke(HttpContext context)
        => await LogResponse(context);

        private async Task LogResponse(HttpContext context)
        {
            if (context.Request.Path.Value == "/health" || context.Request.Path.Value == "/metrics")
                return;
            var service = context.RequestServices.GetService<IHttpContextAccessor>();

            var traceModel = new HttpTraceModel(service);

            context.Request.EnableBuffering();

            var originalBodyStream = context.Response.Body;

            using var responseBody = _recyclableMemoryStreamManager.GetStream();
            {
                context.Response.Body = responseBody;
                var currentExp = await handelNext(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseData = deserializeObj(await new StreamReader(context.Response.Body).ReadToEndAsync(), traceModel.ApiId);
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                if (currentExp != null)
                {
                    context.Items["IsExcp"] = true;
                    Log.Logger.ForContext("Type", "Error")
                                       .ForContext("RequestHeaders",
                                           context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                                           true).ForContext("RequestBody", logRequest(context), true).ForContext("TraceModel", traceModel, true).
                                           ForContext("ResponseBody", responseData, true)
                                       .Error(currentExp, "Response  {RequestMethod} {RequestPath} {statusCode} ",
                                       context.Request.Method, context.Request.Path, context.Response.StatusCode);
                }
                else
                {
                    Log.Logger.ForContext("RequestHeaders",
                                         context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                                         true).ForContext("RequestBody", logRequest(context), true).ForContext("TraceModel", traceModel, true)
                                     .ForContext("ResponseBody",
                                         context.Response.ContentType?.Replace(" ", "") == "application/json;charset=utf-8"
                                             ? responseData
                                             : context.Response.ContentType, true)
                                     .Information("Response information {RequestMethod} {RequestPath} {statusCode} ",
                                         context.Request.Method, context.Request.Path, context.Response.StatusCode
                                     );
                }

                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<Exception> handelNext(HttpContext context)
        {
            Exception currentExp = null;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                currentExp = ex;
                var result = _executeWhenErrorHappen(ex);
                context.Response.Clear();

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(result);
            }

            return currentExp;
        }

        private bool isExcludedApi(string apiId)
        => _excludeFromLogResponse != null && !string.IsNullOrEmpty(apiId)
                           && _excludeFromLogResponse.Contains(apiId);

        private static object logRequest(HttpContext httpContext)
      => (Dictionary<string, object>)httpContext.Items["Args"];

        private object deserializeObj(string myString, string apiId)
        {
            if (isExcludedApi(apiId)) return JsonConvert.DeserializeObject<OperationResult>(myString);
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(myString);
        }
    }
}