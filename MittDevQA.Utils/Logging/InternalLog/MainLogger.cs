//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using Serilog;
//using System;

//namespace MittDevQA.Utils.Logging
//{
//    public class MainLogger
//    {
//        private IHttpContextAccessor _httpContextAccessor;
//        public ILogger _mainLogger { get; private set; }

//        public MainLogger(ILogger mainLogger, IHttpContextAccessor httpContextAccessor)
//        {
//            _httpContextAccessor = httpContextAccessor;
//            _mainLogger = mainLogger;
//        }

//        public void Log(object RequestBody, object ResponseBody, object Message, Exception exception = null)
//        {
//            var logInstance = _httpContextAccessor.HttpContext.GetCloneInstance();
//            logInstance.Message = Message.ToString();
//            logInstance.RequestBody = RequestBody;
//            logInstance.ResponseBody = ResponseBody;
//            _httpContextAccessor.HttpContext.UpdateLog(logInstance);
//            if (exception != null)
//                _mainLogger.ForContext("LogInstance", JsonConvert.SerializeObject(logInstance), true)
//                    .Error(exception, "");
//            else
//                _mainLogger.ForContext("LogInstance", JsonConvert.SerializeObject(_httpContextAccessor.HttpContext.GetLogInstance()), true)
//                    .Information(logInstance.Message.ToString());
//        }
//    }
//}