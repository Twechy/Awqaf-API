using System.Collections.Generic;
using Utils.Senders.Email;

namespace Utils.Senders.MonitorSender
{
    public class MonitorConfiguration : EmailSmtpConfiguration
    {
        public int FailRetryCountInMinutes { get; set; }
        public List<EmailAddress> ToAddresses { get; set; }
        public List<SmsNumber> ToNumbers { get; set; }
    }
}