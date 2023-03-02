using System;

namespace Utils.Localizer.DbLocalizer
{
    public class LocalizationRecord
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Text { get; set; }

        public string LocalizationCulture { get; set; }
        public string ResourceKey { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}