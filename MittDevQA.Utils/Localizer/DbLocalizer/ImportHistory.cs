using System;

namespace Utils.Localizer.DbLocalizer
{
    public class ImportHistory
    {
        public long Id { get; set; }

        public DateTime Imported { get; set; }

        public string Information { get; set; }
    }
}