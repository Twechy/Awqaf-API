using System;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Sinks.PeriodicBatching;

namespace Utils.Logging.InternalLog
{
    public class SemiTransactionLoggger
    {
        private IHttpContextAccessor _httpContextAccessor;
        public ILogger SClogger { get; private set; }

        public SemiTransactionLoggger(ILogger scLogger, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            SClogger = scLogger;
        }

        public void Log(object RequestBody, object ResponseBody, object Message, Exception exception = null, string extraData = null)
        {
            //var logInstance = _httpContextAccessor.HttpContext.GetCloneInstance();
            //logInstance.Message = Message.ToString();
            //logInstance.RequestBody = RequestBody;
            //logInstance.ResponseBody = ResponseBody;
            //logInstance.ExtraData = extraData;

            ////_httpContextAccessor.HttpContext.UpdateLog(logInstance);
            //if (exception != null)
            //    SClogger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
            //                    .Error(exception, "");
            //else
            //    SClogger.ForContext("LogInstance", JsonConvert.SerializeObject(_httpContextAccessor.HttpContext.GetLogInstance()), true)
            //    .Information(logInstance.Message.ToString());
        }
    }

    public class LogOptions
    {
        public LogConfiguration logger { get; set; }
        public PeriodicBatchingSinkOptions Options { get; set; }
    }

    public class LogConfiguration
    {
        public string ConnectionString { get; set; }
        public string LogDBTableName { get; set; }
        public string SemiLogDBTableName { get; set; }
        public string MainLogDBTableName { get; set; }
        public string ControlPanelLogDBTableName { get; set; }
    }
}