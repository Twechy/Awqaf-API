//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using MittDevQA.Logger;
//using Serilog;
//using Serilog.Sinks.PeriodicBatching;

//namespace MittDevQA.Utils.Logging
//{
//    public static class LoggerExtension
//    {
//        public static IApplicationBuilder UseLoggerExtension(this IApplicationBuilder app)
//        {
//            var configuration = app.ApplicationServices.GetService<IConfiguration>();
//            var logOptions = loggerConfiguration(configuration);
//            return app.UseMiddleware<LoggingMiddleware>(logOptions.Options, logOptions.logger.LogDBTableName ?? "LogTable", logOptions.logger.ConnectionString);
//        }

//        public static LogOptions loggerConfiguration(IConfiguration configuration)
//        {
//            var Options = new PeriodicBatchingSinkOptions();
//            configuration.GetSection("LogOptions:BatchingOptions").Bind(Options);
//            var LogOptions = new LogConfiguration();
//            configuration.GetSection("LogOptions:LogConfiguration").Bind(LogOptions);
//            return new LogOptions()
//            {
//                logger = LogOptions,
//                Options = Options
//            };
//        }

//        public static IServiceCollection AddSemiLoggerTransaction(this IServiceCollection services)
//        {
//            services.AddHttpContextAccessor();
//            var HttpContextAccessor = services.BuildServiceProvider().GetService<IHttpContextAccessor>();
//            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
//            var logOptions = loggerConfiguration(configuration);
//            var Logger = (new LoggerConfiguration()
//                .WriteTo.BatchLogger(logOptions.Options, logOptions.logger.SemiLogDBTableName ?? "SemiLogTable", new Logger.ColumnOption.columnOptions(), logOptions.logger.ConnectionString)
//               .CreateLogger());
//            return services.AddSingleton(SClogger => new SemiTransactionLoggger(Logger, HttpContextAccessor));
//        }

//        public static IServiceCollection AddControlPanelTransaction(this IServiceCollection services)
//        {
//            services.AddHttpContextAccessor();
//            var HttpContextAccessor = services.BuildServiceProvider().GetService<IHttpContextAccessor>();
//            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
//            var logOptions = loggerConfiguration(configuration);
//            var Logger = (new LoggerConfiguration()
//                .WriteTo.BatchLogger(logOptions.Options, logOptions.logger.ControlPanelLogDBTableName ?? "ControlPanelLogTable", new Logger.ColumnOption.columnOptions(), logOptions.logger.ConnectionString)
//               .CreateLogger());
//            return services.AddSingleton(SClogger => new ControlPanelLogger(Logger, HttpContextAccessor));
//        }

//        public static IServiceCollection AddMainLogger(this IServiceCollection services)
//        {
//            services.AddHttpContextAccessor();
//            var HttpContextAccessor = services.BuildServiceProvider().GetService<IHttpContextAccessor>();
//            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
//            var logOptions = loggerConfiguration(configuration);
//            var mainLogger = (new LoggerConfiguration()
//                .WriteTo.BatchLogger(logOptions.Options, logOptions.logger.MainLogDBTableName ?? "MainLogTable", new Logger.ColumnOption.columnOptions(), logOptions.logger.ConnectionString)
//               .CreateLogger());
//            return services.AddSingleton(mainlogger => new MainLogger(mainLogger, HttpContextAccessor));
//        }
//    }
//}