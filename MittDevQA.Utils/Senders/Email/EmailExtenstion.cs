using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Utils.Senders.Email
{
    public static class EmailExtenstion
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider())
            {
                var configuration = scope.GetService<IConfiguration>();
                services.Configure<SmtpClientOptions>(configuration.GetSection("EmailSender"));
                services.AddTransient<EmailService>();
            }

            return services;
        }
    }
}