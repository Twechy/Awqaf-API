using HolyQuran.Models;
using HolyQuran.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MittDevQA.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Others;

namespace HolyQuran
{
    public class QuranOptions
    {
        public string FilesLocation { get; set; }
        public string Mp3Location { get; set; }
        public string SheetsLocation { get; set; }
    }

    public static class QuranExtention
    {
        public static void AddQuranServices(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider())
                services.Configure<QuranOptions>(scope.GetService<IConfiguration>().GetSection("quranOptions"));

            services.AddTransient<IStringParserService, FileParserService>()
                .AddTransient<ISurasService, SurasService>()
                .AddTransient<IManagementSurasService, ManagementSurasService>()
                .AddTransient<IReadingService, ReadingService>()
                .AddTransient<ReadersService>()
                .AddTransient<UploadService>()
                .AddTransient<PaginatorService>()
                .AddTransient<QuranCacher>()
                .AddTransient<QuranSearcher>()
                .AddCacheProvider()
                .AddQuranDb();

            using (var scope = services.BuildServiceProvider())
                scope.GetService<QuranCacher>().AddAyat().Wait(CancellationToken.None);
        }
    }
}