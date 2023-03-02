using System;
using Microsoft.Extensions.Localization;
using Utils.Localizer.DbLocalizer;

namespace Utils.Localizer
{
    public class LocalizerService
    {
        private readonly IStringLocalizer _localizer;

        public LocalizerService(IStringExtendedLocalizerFactory factory, Type type)
        {
            _localizer = factory.Create(type);
        }

        public LocalizedString this[string key] => _localizer[key];

        public LocalizedString LocalizedMessage(string key)
        {
            if (key == null) return new LocalizedString("", "");
            return _localizer[key];
        }

        public string LocalizedMessage(string key, params object[] args)
        {
            var localizedString = _localizer[key];
            if (args == null) return localizedString;
            return string.Format(localizedString, args);
        }
    }
}