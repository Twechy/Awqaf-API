using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Utils.Filters
{
    public class ValidateFilterAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            if (context.ModelState.IsValid) return;
            var entry = context.ModelState.Values.FirstOrDefault();

            var message = entry?.Errors.FirstOrDefault()?.ErrorMessage;

            context.Result = new OkObjectResult(new
            {
                Result = -1,
                Data = new JObject(),
                Message = message
            });
        }
    }
}