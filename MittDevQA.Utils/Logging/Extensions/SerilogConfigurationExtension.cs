using System;
using System.Collections.Generic;
using System.Linq;
using Destructurama;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Utils.Logging.Middilewares;

namespace Utils.Logging.Extensions
{
    public static class SerilogConfigurationExtension
    {
        public static IApplicationBuilder UseSerilogMiddleware(this IApplicationBuilder app,
             Func<Exception, string> customReturn = null, string HostName = "",
           List<string> exludesApi = null, bool asMiddleware = true)
        {
            var configuration = app.ApplicationServices.GetService<IConfiguration>();
            var logConfig = configuration.GetConnectionString("LogDB");
            if (logConfig != null)
                Log.Logger = new Serilog.LoggerConfiguration().Destructure.JsonNetTypes().MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{TraceModel} {NewLine}{Exception}")
                    .MinimumLevel.Information()
                    .AuditTo.MSSqlServer(
                    logConfig,
                        $"{HostName}_LOGS",
                        autoCreateSqlTable: true,
                        columnOptions: GetSqlColumnOptions()).CreateLogger();

            return asMiddleware ? app.UseMiddleware<RequestResponseLoggingMiddleware>(customReturn, exludesApi ?? new List<string>()) : app;
        }

        private static ColumnOptions GetSqlColumnOptions()
        {
            var options = new ColumnOptions();

            options.Store.Remove(StandardColumn.Properties);
            options.Store.Add(StandardColumn.LogEvent);
            return options;
        }
    }

    public interface ITraceModel
    {
        string TraceId { get; }
        string ClientIp { get; }
        string UserId { get; }
        string ApiId { get; }
        string ProductId { get; }
        string CustId { get; }

        string AccId { get; }
        string AppId { get; }
    }

    public class HttpTraceModel : ITraceModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _apiId;
        private string _productId;
        private string _appId;
        private string _userId;
        private string _custId;
        private string _accId;

        public HttpTraceModel()
        {
            TraceId = Guid.NewGuid().ToString();
        }

        public HttpTraceModel(IHttpContextAccessor httpContext)
        {
            _httpContextAccessor = httpContext;
            TraceId = Guid.NewGuid().ToString();
            _httpContextAccessor.HttpContext.Items["TraceId"] = TraceId;
        }

        //        correlationId
        public string TraceId { get; }

        public string ClientIp => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string CustId => string.IsNullOrEmpty(_custId)
            ? _custId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "ID")?.Value
            : _custId;

        public string ApiId =>
            string.IsNullOrEmpty(_apiId)
                ? _apiId = _httpContextAccessor.HttpContext?.Items["ApiID"]?.ToString()
                : _apiId;

        public string ProductId => string.IsNullOrEmpty(_productId)
            ? _productId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == "PID")
                ?.Value
            : _productId;

        public string UserId => string.IsNullOrEmpty(_userId)
           ? _userId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == "UID")
               ?.Value
           : _userId;

        public string AppId => string.IsNullOrEmpty(_appId) ?
            _appId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "AppId")?.Value
            : _appId;

        public string AccId => string.IsNullOrEmpty(_accId) ?
            _accId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "ACID")?.Value
            : _accId;
    }
}

//HttpContext.User.FindFirst("PID")?.Value