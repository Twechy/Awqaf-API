using System;
using Utils.Vm;

namespace Utils.Others
{
    public class AppException : Exception
    {
        public bool IsLocalized { get; } = true;

        public string Code { get; }
        public string OtherData { get; }
        public LocalizedMessage LocalizedMessage { get; }
        public object[] MessageParams { get; }

        public AppException(LocalizedMessage localizedMessage, bool isLocalized = true, string data = "")
        {
            LocalizedMessage = localizedMessage;
            OtherData = data;
            IsLocalized = isLocalized;
        }

        public AppException(string message, bool isLocalized = true, params object[] args) : base(message)
        {
            OtherData = message;
            IsLocalized = isLocalized;
            MessageParams = args;
        }

        public AppException(string message, string code, bool isLocalized = true, params object[] args) : base(message)
        {
            OtherData = message;
            MessageParams = args;
            Code = code;
            IsLocalized = isLocalized;
        }

        public AppException(string message, Exception innerExp, params object[] args) : base(message, innerExp)
        {
            OtherData = message;
            MessageParams = args;
        }
    }
}