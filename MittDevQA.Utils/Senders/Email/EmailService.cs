using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Utils.Senders.MonitorSender;

namespace Utils.Senders.Email
{
    public class EmailService
    {
        private readonly IOptions<SmtpClientOptions> _emailOptions;
        private readonly IOptions<MonitorConfiguration> _monitor;

        public EmailService(IOptions<SmtpClientOptions> emailOptions, IOptions<MonitorConfiguration> monitor)
        {
            _emailOptions = emailOptions;
            _monitor = monitor;
            LoadAddresses();
        }

        private List<EmailAddress> ToAddresses { get; set; }

        private void LoadAddresses()
        {
            if (_monitor.Value == null) return;
            if (_monitor.Value.ToAddresses?.Count >= 1) ToAddresses = _monitor.Value.ToAddresses;
        }

        private void SetAddress(EmailContent email, MimeMessage message)
        {
            if (ToAddresses != null) message.To.AddRange(ToAddresses.Select(x => MailboxAddress.Parse(x.Address)));
            else message.To.Add(MailboxAddress.Parse(email.ToEmail));
        }

        public void SendEmail(EmailContent email)
        => createEmailService(email.Identifier).SendEmail(email);

        public StatusCode SendEmail(Notification notification)
        => createEmailService(notification.Identitfier).SendEmail(notification);

        public void SendEmailWithFile(EmailContent email)
        => createEmailService(email.Identifier).SendEmailWithFile(email);

        public void SendEmailWithFileV2(EmailContent email)
        => createEmailService(email.Identifier).SendEmailWithFileV2(email);

        private EmailSender createEmailService(string Identitfier)
        {
            var multiUserNotifierConfig = _emailOptions.Value;
            var stmpOption = multiUserNotifierConfig.ClientOptions.FirstOrDefault(x => x.Identifier == Identitfier);
            var emailSender = new EmailSender(stmpOption);
            return emailSender;
        }
    }

    public class EmailSender
    {
        private readonly SmtpClientOption _stmpOption;

        public EmailSender(SmtpClientOption stmpOption)
      => _stmpOption = stmpOption;

        public EmailSender(IOptions<SmtpClientOption> stmpOption)
         => _stmpOption = stmpOption.Value;

        public void SendEmail(EmailContent email)
        {
            var message = createEmailMessage(email.Subject, email.Content, email.ToEmail);
            sendMail(message);
        }

        public StatusCode SendEmail(Notification notification)
        {
            try
            {
                var message = createEmailMessage(notification.Title, notification.Details, string.Empty);
                sendMail(message);
                return StatusCode.SuccessSend;
            }
            catch
            {
                return StatusCode.FailedToSend;
            }
        }

        public void SendEmailWithFileV2(EmailContent email)
        {
            var message = createEmailMessage(email.Subject, email.Content, email.ToEmail);

            var attachment = new MimePart
            {
                Content = new MimeContent(email.File),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = email.FilePath,
                IsAttachment = true
            };
            message.Body = new Multipart("mixed") { attachment, message.Body };
            sendMail(message);
        }

        public void SendEmailWithFile(EmailContent email)
        {
            var message = createEmailMessage(email.Subject, email.Content, email.ToEmail);
            var attachment = createAttachment(email);
            message.Body = new Multipart("mixed") { attachment, message.Body };
            sendMail(message);
        }

        private void sendMail(MimeMessage message)
        {
            using var emailClient = new SmtpClient();

            if (_stmpOption != null)
            {
                if (_stmpOption.SmtpServer == "smtp.gmail.com")
                    emailClient.Connect(_stmpOption?.SmtpServer, _stmpOption.SmtpPort,
                    true);
                else
                    emailClient.Connect(_stmpOption?.SmtpServer, _stmpOption.SmtpPort,
                      (MailKit.Security.SecureSocketOptions)_stmpOption.SSlType);

                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_stmpOption?.UserName, _stmpOption.Password);
                emailClient.Send(message);
                emailClient.Disconnect(true);
            }
        }

        private MimePart createAttachment(EmailContent email)
        {
            if (email.File == null)
            {
                var filePath = Path.Combine(_stmpOption.StoragePath, email.FilePath);
                var file = File.OpenRead(filePath);
                email.FilePath = Path.GetFileName(file.Name);
                email.File = file;
            }

            var attachment = new MimePart
            {
                Content = new MimeContent(email.File),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = email.FilePath,
                IsAttachment = true
            };
            return attachment;
        }

        private MimeMessage createEmailMessage(string emailTitle, string emailContent, string toEmail)
        {
            var message = new MimeMessage();

            message.From.Add(MailboxAddress.Parse(_stmpOption?.UserName));

            message.Subject = emailTitle;
            message.To.Add(MailboxAddress.Parse(toEmail));

            var body = new TextPart("plain")
            {
                Text = emailContent
            };

            message.Body = body;
            return message;
        }
    }
}