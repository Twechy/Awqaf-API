using System;
using System.Reflection;
using DbUp.Builder;
using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace Utils.ConfigureDatabase
{
    public static class ConfigureDatabase
    {
        public static DbContextOptionsBuilder CreateContext(this DbContextOptionsBuilder optionsBuilder, string databaseType, string ConnectionString)
        {
            return (databaseType == DatabaseType.mysql.ToString()) ?
                 optionsBuilder.UseMySql(ConnectionString) :
                 optionsBuilder.UseSqlServer(ConnectionString);
        }

        public static DbContextOptionsBuilder UseConfigureDatabase(this DbContextOptionsBuilder options, IConfiguration configuration, string ConnectionString, string MigrationsHistoryTable = null)
        {
            if (MigrationsHistoryTable != null)
            {
                var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
                return (databaseType == DatabaseType.mysql) ?
                        options.UseMySql(ConnectionString,
                        x =>
                        {
                            x.MigrationsAssembly("MittDevQA.MySqlServer");
                            x.MigrationsHistoryTable(MigrationsHistoryTable);
                        }) :
                        options.UseSqlServer(ConnectionString,
                        x =>
                        {
                            x.MigrationsAssembly("MittDevQA.SqlServer");
                            x.MigrationsHistoryTable(MigrationsHistoryTable, "migrations");
                        });
            }
            else
            {
                var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
                return (databaseType == DatabaseType.mysql) ?
                    options.UseMySql(ConnectionString, x => x.MigrationsAssembly("MittDevQA.MySqlServer")) :
                    options.UseSqlServer(ConnectionString, x => x.MigrationsAssembly("MittDevQA.SqlServer"));
            }
        }

        public static LoggerConfiguration UseConfigureLogger(this LoggerAuditSinkConfiguration audit, IConfiguration configuration, string connectionString, string tableName)
        {
            var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
            return (databaseType == DatabaseType.mysql) ?
                audit.MySql(connectionString, tableName, autoCreateSqlTable: true, columnOptions: GetMySqlColumnOptions()) :
                audit.MSSqlServer(connectionString, tableName, autoCreateSqlTable: true, columnOptions: GetSqlColumnOptions());
        }

        public static DbContextOptionsBuilder UseConfigureSagaDatabase(this DbContextOptionsBuilder options, IConfiguration configuration, string ConnectionString)
        {
            var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
            return (databaseType == DatabaseType.mysql) ?
                options.UseMySql(ConnectionString) :
                options.UseSqlServer(ConnectionString);
        }

        public static IHealthChecksBuilder UseConfigureHCDatabase(this IHealthChecksBuilder checksBuilder, IConfiguration configuration, string connectionString, string Name = null)
        {
            var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
            return (databaseType == DatabaseType.mysql) ?
                checksBuilder.
                AddMySql(connectionString, name: Name)
                :
                checksBuilder.
                AddSqlServer(connectionString, name: Name);
        }

        private static Serilog.Sinks.MySql.ColumnOptions GetMySqlColumnOptions()
        {
            var options = new Serilog.Sinks.MySql.ColumnOptions();

            options.Store.Remove(Serilog.Sinks.MySql.StandardColumn.Properties);
            options.Store.Add(Serilog.Sinks.MySql.StandardColumn.LogEvent);
            return options;
        }

        private static Serilog.Sinks.MSSqlServer.ColumnOptions GetSqlColumnOptions()
        {
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions();

            options.Store.Remove(Serilog.Sinks.MSSqlServer.StandardColumn.Properties);
            options.Store.Add(Serilog.Sinks.MSSqlServer.StandardColumn.LogEvent);
            return options;
        }

        public static string getDBName(this string connectionString)
       => connectionString.ToLower().Split(new string[] { "database" }, StringSplitOptions.None)[1]
                                  .Split(';')[0].Replace("=", "")
                                  .Trim();

        public static UpgradeEngineBuilder quartzConfigure(this SupportedDatabases to, IConfiguration configuration, string connectionString, string scriptDirctory)
        {
            var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
            if (databaseType == DatabaseType.mysql)
            {
                return to.MySqlDatabase(connectionString, connectionString.getDBName())
                    .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly(),
                                                    (string Script) => Script.StartsWith(scriptDirctory + ".mysql.mysql_core_jobs.sql"));
            }
            else
            {
                return to.SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetEntryAssembly(),
                                                    (string Script) => Script.StartsWith(scriptDirctory + ".sqlserver.core_jobs.sql"));
            }
        }

        public static IGlobalConfiguration HangfireConfigure(this IGlobalConfiguration hanfireConfig, IConfiguration configuration, string connectionString)
        {
            var databaseType = configuration.GetValue<DatabaseType>("DatabaseType:Type");
            if (databaseType == DatabaseType.mysql)
            {
                return hanfireConfig.UseStorage<MySqlStorage>(new MySqlStorage(connectionString,
                    new MySqlStorageOptions()
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(0.0001),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        DashboardJobListLimit = 50000,
                        InvisibilityTimeout = TimeSpan.FromMinutes(5),
                        TablesPrefix = "Hangfire."
                    }));
            }
            else
            {
                return hanfireConfig.UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });
            }
        }

        public enum DatabaseType
        {
            sqlserver,
            mysql
        }
    }
}