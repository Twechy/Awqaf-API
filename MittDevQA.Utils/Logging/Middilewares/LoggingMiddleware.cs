//using Microsoft.AspNetCore.Http;
//using MittDevQA.Logger;
//using Newtonsoft.Json;
//using Serilog;
//using Serilog.Sinks.PeriodicBatching;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace MittDevQA.Utils
//{
//    public class LoggingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly PeriodicBatchingSinkOptions _options;
//        private readonly string _tableName;
//        private readonly string _connectionString;
//        private ILogger _logger;

//        public LoggingMiddleware(RequestDelegate next, PeriodicBatchingSinkOptions options, string tableName, string connectionString)
//        {
//            _next = next;
//            _options = options;
//            _tableName = tableName;
//            _connectionString = connectionString;
//        }

//        public async Task Invoke(HttpContext context)
//        {
//            _logger ??= (new LoggerConfiguration()
//                .WriteTo.BatchLogger(_options, _tableName, new Logger.ColumnOption.columnOptions(), _connectionString)
//               .CreateLogger());

//            try
//            {
//                await _next(context);
//                var logInstance = context.GetLogInstance();
//                logInstance.Message = $"{context.Request.Method}{context.Request.Path} {context.Response.StatusCode}";
//                _logger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
//                    .Information(logInstance.Message.ToString());
//            }
//            catch (AppException ex)
//            {
//                context.Response.ContentType = "application/json";
//                var logInstance = context.GetLogInstance();
//                var responseData = logInstance.GetDyanmicResponse();
//                var result = new { type = responseData.Type, messages = ex.Message, traceId = logInstance.TraceId };
//                logInstance.Message = $"{context.Request.Method}{context.Request.Path} {context.Response.StatusCode}";
//                _logger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
//                    .Information(logInstance.Message.ToString());
//                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//            }
//            catch (Exception ex)
//            {
//                //  const int code = (int)HttpStatusCode.InternalServerError;
//                context.Response.ContentType = "application/json";

//                var logInstance = context.GetLogInstance();
//                logInstance.Message = $"{context.Request.Method}{context.Request.Path} {context.Response.StatusCode}";
//                _logger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
//                    .Error(ex, "");
//                var responseData = logInstance.GetDyanmicResponse();
//                var result = new { type = responseData.Type, messages = responseData.Messages, traceId = logInstance.TraceId };
//                await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
//            }
//            finally
//            {
//                ///Log.Logger. Information("Request {method} {url} => {statusCode}", context.Request?.Method, context.Request?.Path.Value, context.Response?.StatusCode);
//            }
//        }
//    }

//    public static class GetLogInstenceClass
//    {
//        public static LogInstance GetLogInstance(this HttpContext context)
//        {
//            if (context.Items["LogInstance"] == null) context.Items.Add("LogInstance", new LogInstance());
//            return (LogInstance)context.Items["LogInstance"];
//        }

//        public static LogInstance GetCloneInstance(this HttpContext context)
//        {
//            var instance = GetLogInstance(context);
//            return new LogInstance
//            {
//                ApiId = instance.ApiId,
//                AppId = instance.AppId,
//                IdentityAccount = instance.IdentityAccount,
//                Message = instance.Message,
//                ProductId = instance.ProductId,
//                RequestBody = instance.RequestBody,
//                ResponseBody = instance.ResponseBody,
//                TraceId = instance.TraceId
//            };
//        }

//        public static void UpdateLog(this HttpContext httpContext, LogInstance instance)
//        {
//            var currentContext = GetLogInstance(httpContext);

//            currentContext.ApiId = int.Parse(httpContext.Items["ApiID"]?.ToString() ?? 0.ToString());
//            currentContext.ProductId = int.Parse(httpContext.User.FindFirst("PID")?.Value ?? instance.ProductId.ToString());
//            currentContext.RequestBody = instance.RequestBody ?? currentContext.RequestBody;
//            currentContext.ResponseBody = instance.ResponseBody ?? currentContext.ResponseBody;
//            currentContext.TraceId = httpContext.Items["OverrideTraceId"]?.ToString() ?? instance.TraceId ?? httpContext.Items["TraceId"]?.ToString();
//            currentContext.Message = currentContext.Message ?? instance.Message;
//            currentContext.AppId = httpContext.User.FindFirst("AppId")?.Value ?? instance.AppId ?? instance.IdentityAccount;
//            currentContext.IdentityAccount = getUId(httpContext) ??
//                httpContext.User?.FindFirst("ID")?.Value ??
//                 instance.IdentityAccount ?? httpContext.User?.FindFirst("AppId")?.Value;
//            currentContext.ExtraData = instance.ExtraData ?? null;
//        }

//        private static string getUId(HttpContext httpContext) =>
//              httpContext.User?.FindFirst("UID")?.Value == null ||
//             (httpContext.User?.FindFirst("UID")?.Value.Contains("-1") == true ||
//              httpContext.User?.FindFirst("UID")?.Value.Contains("0") == true) ? null :
//              httpContext.User?.FindFirst("UID")?.Value;
//    }
//}