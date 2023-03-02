using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Utils.Localizer.DbLocalizer
{
    // >dotnet ef migrations add LocalizationMigration
    public class DevelopmentSetup
    {
        private readonly LocalizationModelContext _context;
        private readonly IOptions<SqlLocalizationOptions> _options;
        private IOptions<RequestLocalizationOptions> _requestLocalizationOptions;

        public DevelopmentSetup(
           LocalizationModelContext context,
           IOptions<SqlLocalizationOptions> localizationOptions,
           IOptions<RequestLocalizationOptions> requestLocalizationOptions)
        {
            _options = localizationOptions;
            _context = context;
            _requestLocalizationOptions = requestLocalizationOptions;
        }

        public void AddNewLocalizedItem(string key, string culture, string resourceKey)
        {
            if (_requestLocalizationOptions.Value.SupportedCultures.Contains(new System.Globalization.CultureInfo(culture)))
            {
                try
                {
                    lock (lockObj)
                    {
                        var entries = _context.ChangeTracker.Entries().ToList();

                        string computedKey = $"{key}.{culture}";

                        var localizationRecord = new LocalizationRecord()
                        {
                            LocalizationCulture = culture,
                            Key = key,
                            Text = computedKey,
                            ResourceKey = resourceKey
                        };
                        var entry = _context.Entry(localizationRecord);
                        if (entry != null) entry.State = EntityState.Detached;
                        _context.LocalizationRecords.Add(localizationRecord);
                        _context.SaveChanges();
                    }
                }
                catch { }
            }
        }

        private static object lockObj = new object();
    }
}