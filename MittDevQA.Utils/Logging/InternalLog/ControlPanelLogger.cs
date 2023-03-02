//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using Serilog;
//using System;

//namespace MittDevQA.Utils.Logging
//{
//    public class ControlPanelLogger
//    {
//        private IHttpContextAccessor _httpContextAccessor;
//        public ILogger SClogger { get; private set; }

//        public ControlPanelLogger(ILogger scLogger, IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//            SClogger = scLogger;
//        }

//        public void Log(object RequestBody, object ResponseBody, object Message, Exception exception = null)
//        {
//            var logInstance = _httpContextAccessor.HttpContext.GetCloneInstance();
//            logInstance.Message = Message.ToString();
//            logInstance.RequestBody = RequestBody;
//            logInstance.ResponseBody = ResponseBody;
//            _httpContextAccessor.HttpContext.UpdateLog(logInstance);
//            if (exception != null)
//                SClogger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
//                                .Error(exception, "");
//            else
//                SClogger.ForContext("LogInstance", JsonConvert.SerializeObject(_httpContextAccessor.HttpContext.GetLogInstance()), true)
//                .Information(logInstance.Message.ToString());
//        }
//    }
//}