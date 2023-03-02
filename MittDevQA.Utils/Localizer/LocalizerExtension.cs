using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Utils.Localizer.DbLocalizer;

namespace Utils.Localizer
{
    public static class LocalizerExtension
    {
        public static IServiceCollection AddLocalizer(this IServiceCollection services, Type sharedResourcesAssembly,
            bool isAspHost = true)
        {
            string sqlConnectionString = null;
            bool createIfNotExist = false;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var config = serviceProvider.GetService<IConfiguration>();
                sqlConnectionString = config.GetConnectionString("LocalizerDB");
                createIfNotExist = config.GetValue<bool>("LocalizerOptions:EnableInsertInDbIfNotFound");
            }
            if (createIfNotExist && !isAspHost)
            {
                services.AddDbContextPool<LocalizationModelContext>(options =>
                    options.UseSqlServer(sqlConnectionString,
                        b =>
                        {
                            b.MigrationsAssembly("MittDevQA.Utils");
                            b.MigrationsHistoryTable("__MigrationsHistoryForLocalizerService", "migrations");
                        }));
            }
            else
            {
                services.AddDbContext<LocalizationModelContext>(options =>
                 options.UseSqlServer(sqlConnectionString,
                     b =>
                     {
                         b.MigrationsAssembly("MittDevQA.Utils");
                         b.MigrationsHistoryTable("__MigrationsHistoryForLocalizerService", "migrations");
                     }), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

                //services.AddDbContext<LocalizationModelContext>(
                //options =>
                //    options.UseSqlServer(sqlConnectionString,
                //        b =>
                //        {
                //            b.MigrationsAssembly("MittDevQA.Utils");
                //            b.MigrationsHistoryTable("__MigrationsHistoryForLocalizerService", "migrations");
                //        }), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
            }
            using (var provider = services.BuildServiceProvider())
            {
                var localizerDB = provider.GetService<LocalizationModelContext>();
                localizerDB.Database.Migrate();
            }

            // Requires that LocalizationModelContext is defined
            services.AddSqlLocalization(options =>
            {
                options.UseTypeFullNames = true;
                options.ReturnOnlyKeyIfNotFound = true;
                options.CreateNewRecordWhenLocalisedStringDoesNotExist = createIfNotExist;
            }).AddTransient(x => new LocalizerService(x.GetService<IStringExtendedLocalizerFactory>(), sharedResourcesAssembly));
            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                        {
                            new CultureInfo("en-US"),
                            new CultureInfo("ar-LY")
                        };
                    options.DefaultRequestCulture = new RequestCulture(culture: "ar-LY", uiCulture: "ar-LY");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });
            return services;
        }

        public static IApplicationBuilder UseRequestLocalizer(this IApplicationBuilder app)
        {
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);
            return app;
        }
    }

    public enum LocalizerContextType
    {
        Singleton,
        Transient
    }

    public class LocalizerOptions
    {
        public bool EnableInsertInDbIfNotFound { get; set; }
    }
}