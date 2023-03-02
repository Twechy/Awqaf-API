using System;

namespace Utils.Logging.InternalLog;

public class LogOptions<T>
{
    public bool LogContent { get; set; } = true;
    public dynamic ReqLogs { get; set; } = null;
    public Func<T, dynamic, string> IdentityFunc { get; set; } = null;
    public int? ProductId { get; set; } = -1;
    public string AppId { get; set; } = null;
    public string TraceId { get; set; } = null;
}