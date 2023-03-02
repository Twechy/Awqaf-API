using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Db;

public static class AwqafDbExt
{
    public static IServiceCollection AddAwqafDb(this IServiceCollection services)
    {
        string connectionString;

        using (var scope = services.BuildServiceProvider()) connectionString = scope
                .GetService<IConfiguration>()
                .GetConnectionString(nameof(AwqafDb));

        if (string.IsNullOrEmpty(connectionString)) throw new Exception($"connection string for {nameof(AwqafDb)} is missing from the appsettings.json file!!");

        services.AddDbContextPool<AwqafDb>(opt => opt.UseSqlServer(connectionString, x => x.MigrationsHistoryTable($"__MigrationsHistoryFor{nameof(AwqafDb)}", "migrations"))
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging());

        using (var scope = services.BuildServiceProvider()) scope.GetService<AwqafDb>()?.Database?.Migrate();

        return services;
    }
}