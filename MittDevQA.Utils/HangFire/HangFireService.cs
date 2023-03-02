using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Mvc;

namespace Utils.HangFire
{
    public class HangFireConfig
    {
        public int[] RetrySpans { get; set; }
        public int RetryAttempts { get; set; }
        public string ConnectionString { get; set; }
    }

    public class HangFireService
    {
        private readonly IBackgroundJobClient _jobClient;

        public HangFireService(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }

        // [AutomaticRetry (Attempts = 5, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail,
        //     DelaysInSeconds = new [] { 15, 25, 35 })]
        public string ScheduleTask(Expression<Action> methodCall) =>
            _jobClient.Schedule(methodCall, DateTimeOffset.Now);

        // [AutomaticRetry (Attempts = 5, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail,
        //     DelaysInSeconds = new [] { 15, 25, 35 })]
        public string ScheduleWithSpan(Expression<Action> methodCall, TimeSpan timeSpan) =>
            _jobClient.Schedule(methodCall, timeSpan);
    }

    public static class HangExtenstion
    {
        public static IServiceCollection AddHangExtenstion(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();

                services.Configure<HangFireConfig>(configuration.GetSection("HangFireConfig"));
                var fireConfig = configuration.GetOptions<HangFireConfig>("HangFireConfig");

                if (fireConfig.RetryAttempts > 0)
                {
                    object automaticRetryAttribute = null;
                    foreach (var filter in GlobalJobFilters.Filters)
                    {
                        if (!(filter.Instance is AutomaticRetryAttribute)) continue;
                        automaticRetryAttribute = filter.Instance;
                        Trace.TraceError("Found HangFire automatic retry");
                    }

                    GlobalJobFilters.Filters.Remove(automaticRetryAttribute);
                }

                services.AddHangfire(
                    x =>
                    {
                        x.UseSqlServerStorage(fireConfig.ConnectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            UsePageLocksOnDequeue = true,
                            DisableGlobalLocks = true
                        });

                        x.UseColouredConsoleLogProvider();
                        x.UseColouredConsoleLogProvider();
                    }
                );

                if (fireConfig.RetryAttempts > 0)
                {
                    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
                    {
                        Attempts = fireConfig.RetryAttempts,
                        DelaysInSeconds = fireConfig.RetrySpans,
                        LogEvents = true,
                        OnAttemptsExceeded = AttemptsExceededAction.Fail
                    });
                }
            }

            services.AddTransient<HangFireService>();
            return services;
        }

        public static IApplicationBuilder UseHangFireExtenstion(this IApplicationBuilder app)
        {
            var options = new BackgroundJobServerOptions { WorkerCount = Environment.ProcessorCount * 5 };

            return app.UseHangfireServer(options)
            .UseHangfireDashboard(
                "/jobs", new DashboardOptions()
            /*{
                Authorization = new[]
                {
                    new HangFireAuthorization(app.ApplicationServices.GetService<IAuthorizationService>(),
                        app.ApplicationServices.GetService<IHttpContextAccessor>(), admin)
                }
            }*/
            );
        }
    }
}