using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Utils.Others
{
    public static class CacheExtenstion
    {
        public static IServiceCollection AddCacheProvider(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var config = serviceProvider.GetService<IConfiguration>();
                var redisConnectionString = config.GetValue<string>("RedisConfig:connectionString");
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.Configuration = redisConnectionString;
                });
                var configuration = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
                };
                configuration.EndPoints.Add(redisConnectionString);
                var connection = ConnectionMultiplexer.Connect(configuration.ToString());
                services.AddTransient((svc) => new CashLocker(connection));
            }

            services.AddTransient<CacheProvider>();
            return services;
        }
    }

    public class CashLocker
    {
        private readonly ConnectionMultiplexer _cashConnection;

        public CashLocker(ConnectionMultiplexer connectionMultiplexer)
        {
            _cashConnection = connectionMultiplexer;
        }

        public bool AcquireLock(string key, string value, TimeSpan expiration)
        {
            bool flag = false;
            try
            {
                flag = _cashConnection.GetDatabase().
                    StringSet(key, value, expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                var x = 10;
            }
            return flag;
        }
    }

    public class RedisConfig
    {
        /// public string connectionString { get; set; }
        public TimeSpan startTransactionTimeout { get; set; }

        public TimeSpan confirmTransactionTimeout { get; set; }
        public TimeSpan startMigrateTimeout { get; set; }
        public TimeSpan confirmMigrateTimeout { get; set; }
        public TimeSpan smsSeqNOTimeout { get; set; }
    }

    public class CacheProvider
    {
        private readonly IDistributedCache _cache;

        public CacheProvider(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task AddToCache<T>(string key, T value, TimeSpan? span = null) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            if (value is null) throw new AppException("value is null..");
            InvalidateCache(key);
            if (span == null)
                await _cache.SetStringAsync(key, value.ToJson());
            else
                await _cache.SetStringAsync(key, value.ToJson(),
                 new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = span });
        }

        public void AddToCacheAsString(string key, string value, TimeSpan? span = null)
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            if (value is null) throw new AppException("value is null..");
            if (span == null)
                _cache.SetString(key, value);
            else
                _cache.SetString(key, value,
                  new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = span });
        }

        public async Task AddToCacheAsStringAsync(string key, string value, TimeSpan? span = null)
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            if (value is null) throw new AppException("value is null..");
            if (span == null)
                await _cache.SetStringAsync(key, value);
            else
                await _cache.SetStringAsync(key, value,
                   new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = span });
        }

        public void InvalidateCache(string key)
        {
            if (!string.IsNullOrEmpty(key))
                _cache.Remove(key);
        }

        public async Task InvalidateCacheAsync(string key)
        {
            if (!string.IsNullOrEmpty(key))
                await _cache.RemoveAsync(key);
        }

        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            return _cache.GetString(key);
        }

        public async Task<string> GetStringAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            return await _cache.GetStringAsync(key);
        }

        public async Task<T> GetObject<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new AppException("key is null..");
            return HandleDeserialize<T>(await _cache.GetStringAsync(key));
        }

        private static T HandleDeserialize<T>(string value) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception e)
            {
                throw new AppException("can't serialize this object..", e.Message);
            }
        }
    }
}