using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Utils.Mvc
{
    public class AutomaticCustomValidator<T> : AbstractValidator<T>
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AutomaticCustomValidator(IHttpContextAccessor contextAccessor)
            => _contextAccessor = contextAccessor;

        public override ValidationResult Validate(ValidationContext<T> context)
        {
            var baseValidation = base.Validate(context);

            if (!baseValidation.IsValid)
            {
                var errorMessages = baseValidation.ToString().Split("\r\n");

                _contextAccessor.HttpContext.Items.Add("validationErrors", errorMessages.ToList());
            }

            return new ValidationResult();
        }

        public override async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
        {
            var baseValidation = await base.ValidateAsync(context, cancellation);

            if (!baseValidation.IsValid)
            {
                var errorMessages = baseValidation.ToString().Split("\r\n");

                _contextAccessor.HttpContext.Items.Add("validationErrors", errorMessages.ToList());
            }

            return new ValidationResult();
        }
    }
}