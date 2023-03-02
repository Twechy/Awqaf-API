using System.IO;

namespace Utils.Senders.Email
{
    public class EmailAddress
    {
        public string Address { get; set; }
    }

    public class SmsNumber
    {
        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class EmailContent
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public string ToEmail { get; set; }

        public string Identifier { get; set; }

        public Stream File { get; set; } = null;
    }

    public class Notification
    {
        public string CustomerId { get; set; }
        public long Id { get; set; }
        public string Details { get; set; }
        public string OccuredTime { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }

        public string Identitfier { get; set; }

        private Notification()
        {
        }

        public static Notification Create(long id, string title, string detials,
            string occureTime, string address)
        {
            return new Notification
            {
                Id = id,
                OccuredTime = occureTime,
                Title = title,
                Details = detials,
                Address = address
            };
        }
    }

    public enum StatusCode
    {
        SuccessSend,
        FailedToSend
    }
}