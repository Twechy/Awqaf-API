using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Utils.Others;
using Utils.Senders.Email;
using Utils.Senders.Sms;

namespace Utils.Senders.MonitorSender
{
    public class MonitorSender
    {
        private readonly CacheProvider _cache;
        private readonly EmailService _emailService;
        private readonly IOptions<MonitorConfiguration> _monitorConfiguration;
        private readonly SmsSender _smsSender;
        private int RetryCount { get; } = 3;

        public MonitorSender(IOptions<MonitorConfiguration> configuration, EmailService emailService,
            SmsSender smsSender, CacheProvider cache)
        {
            _monitorConfiguration = configuration;
            _emailService = emailService;
            _smsSender = smsSender;
            _cache = cache;
            if (_monitorConfiguration?.Value.FailRetryCountInMinutes != null)
                RetryCount = (int)_monitorConfiguration?.Value?.FailRetryCountInMinutes;
        }

        /// <summary>
        ///     sends an email and an sms message to the client list specified in the appsetting file
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public async Task NotifyClients(string title, string message)
        {
            var key = $"{title}-{message}";

            var cachedMessage = _cache.GetString(key);
            if (cachedMessage == null) throw new AppException("cached message is null..");

            await SendEmailNotification(title, message);
            await SendSmsNotification(title);
            await _cache.AddToCache(key, cachedMessage, TimeSpan.FromMinutes(RetryCount));
        }

        private Task SendEmailNotification(string title, string message)
        {
            _emailService.SendEmail(
                new EmailContent
                {
                    Content = message,
                    Subject = title
                });
            return Task.CompletedTask;
        }

        private Task SendSmsNotification(string title)
        {
            _monitorConfiguration.Value.ToNumbers
                .ForEach(async number => await _smsSender.SendSms(number.Number, title));
            return Task.CompletedTask;
        }
    }
}