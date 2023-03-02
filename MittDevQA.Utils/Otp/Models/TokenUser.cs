using System;

namespace Utils.Otp.Models
{
    public class TokenUser
    {
        public int? Sn { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int? Bankid { get; set; }
        public int? Branchid { get; set; }
        public int? Statu { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? ActDate { get; set; }
        public DateTime? DeActDate { get; set; }
    }
}