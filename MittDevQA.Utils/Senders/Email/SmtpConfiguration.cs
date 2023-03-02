namespace Utils.Senders.Email
{
    public class EmailSmtpConfiguration
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
    }

    public class SmtpClientOptions
    {
        public SmtpClientOption[] ClientOptions { get; set; }
    }

    public class SmtpClientOption
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string Identifier { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string StoragePath { get; set; }

        public int SSlType { get; set; } = 1;
    }
}