using System;

namespace Utils.Localizer.DbLocalizer
{
    public class ExportHistory
    {
        public long Id { get; set; }

        public DateTime Exported { get; set; }

        public string Reason { get; set; }
    }
}