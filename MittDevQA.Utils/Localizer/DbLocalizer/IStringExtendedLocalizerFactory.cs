using System;
using Microsoft.Extensions.Localization;

namespace Utils.Localizer.DbLocalizer
{
    public interface IStringExtendedLocalizerFactory : IStringLocalizerFactory
    {
        void ResetCache();

        void ResetCache(Type resourceSource);
    }
}