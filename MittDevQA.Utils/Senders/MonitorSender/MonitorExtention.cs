using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.HangFire;
using Utils.Senders.Sms;

namespace Utils.Senders.MonitorSender
{
    public static class MonitorExtention
    {
        public static IServiceCollection AddMonitorSender(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider())
            {
                var configuration = scope.GetService<IConfiguration>();

                services.Configure<MonitorConfiguration>(configuration.GetSection("MonitorConfig"))
                    .AddTransient<MonitorSender>()
                    .AddTransient<SmsSender>()
                    .AddTransient<HangFireService>();
            }

            return services;
        }
    }
}