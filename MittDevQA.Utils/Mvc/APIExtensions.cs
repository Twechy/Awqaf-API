using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Utils.Filters;
using Utils.Logging.Extensions;
using Utils.Others;
using Utils.Vm;
using LocalizerService = Utils.Localizer.LocalizerService;

namespace Utils.Mvc
{
    public static class APIExtensions
    {
        public static string Underscore(this string value)
        {
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
        }

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(section).Bind(model);
            return model;
        }

        public static IServiceCollection AddMvcConfig(
            this IServiceCollection services,
            IModelBinderProvider[] modelBindersProviders = null,
            bool useApiIdValidator = true,
            params Assembly[] asmForMapperAndValidators)
        {
            services.AddHttpContextAccessor();
            services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(5); });

            services.AddHealthChecks();
            void MvcOptionAction(MvcOptions ops)
            {
                if (modelBindersProviders == null) return;
                foreach (var binderProvider in modelBindersProviders)
                    ops.ModelBinderProviders.Insert(0, binderProvider);
            }

            if (asmForMapperAndValidators.Length > 0)
                services.AddAutoMapper(asmForMapperAndValidators)
                    .AddMvc(MvcOptionAction)
                    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(asmForMapperAndValidators));
            else
                services.AddMvc(MvcOptionAction);

            services.AddMvcCore();

            if (asmForMapperAndValidators.Length > 0)
                services.Configure<ApiBehaviorOptions>(o =>
                {
                    o.SuppressModelStateInvalidFilter = useApiIdValidator;

                    var serviceProvider = services.BuildServiceProvider();
                    var localizer = serviceProvider.GetService<LocalizerService>();
                    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

                    o.InvalidModelStateResponseFactory = context =>
                    {
                        return new ObjectResult(OperationResult.UnValid(
                            context.ModelState.Values.Where(w =>
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("Path") &&
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("line") &&
                            (bool)!w.Errors.FirstOrDefault()?.ErrorMessage?.Contains("position")).Select(x =>
                             localizer.LocalizedMessage(x.Errors.FirstOrDefault()?.ErrorMessage).ToString()).ToList()
                       ));
                    };
                });

            services.AddTransient(_ => Log.Logger);

            services.AddCors(
                options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .WithExposedHeaders("Content-Length", "Access-Control-Allow-Origin", "TotalRecords",
                                "Origin", "Token-Expired")
                            .AllowCredentials()
                            .AllowAnyHeader());
                });

            return services;
        }

        public static IApplicationBuilder UseMvcConfig(this IApplicationBuilder app, bool useLoggingHandler = false
            , Action<IApplicationBuilder> funcApp = null, string appName = "", List<string> exludesApi = null, bool asMiddleWare = true)
        {
            var localizer = app.ApplicationServices.GetService<LocalizerService>();
            var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();

            if (useLoggingHandler)
            {
                if (appName != "")
                    app.UseSerilogMiddleware(e =>
                    {
                        var messages = new List<string>();
                        OperationResult operationResult;
                        if (e is AppException exception)
                        {
                            var localizedString = !exception.IsLocalized ? exception.Message : exception.MessageParams != null
                                ? localizer.LocalizedMessage(exception.Message, exception.MessageParams)
                                : localizer.LocalizedMessage(exception.Message);

                            messages.Add(localizedString);
                            operationResult = OperationResult.UnValid(messages);
                        }
                        else
                        {
                            messages.Add(localizer.LocalizedMessage("حدثت_مشكلة_في_الخادم_الرجاء_الاتصال_بالدعم_الفني"));
                            operationResult = OperationResult.UnValid(messages,
                                OperationResult.ResultType.TechError);
                        }

                        operationResult.TraceId = httpContextAccessor.HttpContext.Items["TraceId"]?.ToString();

                        return operationResult.ToJson();
                    }, appName, exludesApi, asMiddleWare);
                else
                {
                    //app.UseLoggerExtension();
                }
            }
            else
                app.UseMiddleware<ExceptionHandler>();

            funcApp?.Invoke(app);
            app.UseHealthChecks("/health");
            app.UseRouting();
            app.UseAuthentication().UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            }); ;

            return app;
        }
    }

    public enum ExceptionHandlerType
    {
        LoggingHandler,
        ApiExceptionHandler
    }
}