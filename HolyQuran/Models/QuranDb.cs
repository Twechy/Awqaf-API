using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HolyQuran.Models
{
    public static class QuranDbExtenstions
    {
        public static void AddQuranDb(this IServiceCollection services)
        {
            string dbConnectionString;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                dbConnectionString = configuration.GetConnectionString("QuranDb");

                services.AddDbContext<QuranDb>(options =>
                   options.UseSqlServer(dbConnectionString));
            }

            using (var serviceProvider = services.BuildServiceProvider())
            {
                serviceProvider.GetService<QuranDb>()
                    .Database
                    .Migrate();
            }
        }
    }

    public class QuranDb : DbContext
    {
        public DbSet<Sorah> Suras { get; set; }
        public DbSet<Ayah> Ayat { get; set; }

        public DbSet<HolyReadingModel> Reading { get; set; }
        public DbSet<HolyText> RawyText { get; set; }
        public DbSet<Reader> Readers { get; set; }

        public QuranDb()
        { }

        public QuranDb(DbContextOptions<QuranDb> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            optionsBuilder.UseSqlServer("Server=.;database=QuranDb;User ID=ans;Password=AnsTwechy123;", options =>
            {
                options.EnableRetryOnFailure();
            });

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }
}