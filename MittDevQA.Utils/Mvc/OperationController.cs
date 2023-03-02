using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Utils.Vm;

namespace Utils.Mvc
{
    public class OperationController : ControllerBase
    {
        protected static OperationResult<T> ErrorOperation<T>(string message = null, string traceId = null)
        {
            return OperationResult<T>.UnValid(message is null
                ? new List<string>()
                : new List<string>
                {
                    message
                }, traceId);
        }

        protected static OperationResult<T> OkOperation<T>(string message = null, string traceId = null)
        {
            return OperationResult<T>.Valid(default, message is null
                ? new List<string>()
                : new List<string>
                {
                    message
                }, traceId);
        }

        protected static OperationResult<T> OkOperation<T>(T instance)
        {
            return OperationResult<T>.Valid(instance);
        }

        protected static OperationResult ErrorOperation(string message = null, string traceId = null)
        {
            return OperationResult.UnValid(message is null
                ? null
                : new List<string>
                {
                    message
                }, traceId: traceId);
        }

        protected static async Task<OperationResult<T>> OkOperationResult<T>(Func<Task<T>> action,
            string message = null, string traceId = null)
            => OperationResult<T>.Valid(await action(),
                message is null
                    ? new List<string>()
                    : new List<string>
                    {
                        message,
                    }, traceId);

        protected static async Task<OperationResult> OkOperationResult(Func<Task> action,
            string message = null, string traceId = null)
        {
            await action();
            return OperationResult.Valid(
                message is null
                    ? new List<string>()
                    : new List<string>
                    {
                        message
                    }, traceId);
        }

        protected static OperationResult<T> OkOperation<T>(Func<T> action, string message = null, string traceId = null)
            => OperationResult<T>.Valid(action.Invoke(),
                message is null
                    ? new List<string>()
                    : new List<string>
                    {
                        message
                    }, traceId);

        protected static OperationResult OkOperation(string message = null, string traceId = null)
        {
            return OperationResult.Valid(
                message is null
                    ? new List<string>()
                    : new List<string>
                    {
                        message
                    }, traceId
            );
        }

        protected static async Task<OperationResult> OkOperation(Func<Task> action,
        string message = null, string traceId = null)
        {
            await action();
            return OperationResult.Valid(
                  message is null
                      ? new List<string>()
                      : new List<string>
                      {
                        message
                      }, traceId);
        }
    }
}