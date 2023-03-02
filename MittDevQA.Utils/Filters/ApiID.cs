using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Utils.Localizer;
using Utils.Vm;

namespace Utils.Filters
{
    public class APIIDAttribute : ActionFilterAttribute
    {
        private readonly object _actionId;
        private readonly bool _excludeParamsFromLogs;
        private readonly bool _showAllMessages;

        public APIIDAttribute(object actionId, bool excludeParamsFromLogs = false, bool showAllMessages = false)
        {
            _actionId = actionId;
            _excludeParamsFromLogs = excludeParamsFromLogs;
            _showAllMessages = showAllMessages;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var args = _excludeParamsFromLogs ? new Dictionary<string, object>() :
               new Dictionary<string, object>(context.ActionArguments?.Select(x => new KeyValuePair<string, object>(x.Key, x.Value)));
            context.HttpContext.Items["Args"] = args;
            context.HttpContext.Items["ApiID"] = (long)_actionId;

            var TraceId = Guid.NewGuid().ToString();
            context.HttpContext.Items["TraceId"] = TraceId;
            var localizer = context.HttpContext.RequestServices.GetService<LocalizerService>();
            if (!context.ModelState.IsValid)
            {
                var traceId = context.HttpContext.Items["TraceId"]?.ToString();
                context.Result = _showAllMessages ? new ObjectResult(OperationResult.UnValid(traceId,
                      context.ModelState.Values.SelectMany(x => x.Errors?.Select(e => localizer.LocalizedMessage(e?.ErrorMessage).ToString()).ToList()).ToArray()))
                     : new ObjectResult(OperationResult.UnValid(traceId,
                            context.ModelState.Values.Where(w =>
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("Path") &&
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("line") &&
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("position")).Select(x =>
                             localizer.LocalizedMessage(x.Errors.FirstOrDefault()?.ErrorMessage).ToString()).ToArray()));
            }

            base.OnActionExecuting(context);
        }
    }
}