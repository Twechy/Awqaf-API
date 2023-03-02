using System;
using System.Collections.Generic;
using System.Text;

namespace HolyQuran.Vms
{
    public class UpdateReadDto
    {
        public int Id { get; set; }
        public int AyaNumber { get; set; }
        public string Reader { get; set; }
        public string ReadView { get; set; }
        public string HolyRead { get; set; }
        public string ReadInfo { get; set; } = string.Empty;
        public bool AgreedOn { get; set; }
    }
}
