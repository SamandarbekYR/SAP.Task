using Microsoft.Extensions.Caching.Distributed;
using SAP.API.Services.Interfaces;

namespace SAP.API.Services.Service
{
    public class RedisService : IRedis
    {
        private IDistributedCache _cache;
        private ILogger<RedisService> _logger;

        public RedisService(IDistributedCache cache, ILogger<RedisService> logger)
        {
            this._cache = cache;
            this._logger = logger;
        }
        public async Task DeleteAsync(string key)
        {
            _logger.LogInformation("Cachedan malumotlar tozalanmoqda ...");
            await _cache.RemoveAsync(key);
        }

        public async Task<string?> GetString(string key)
            => await _cache.GetStringAsync(key);

        public async Task SetAsync(string key, string value)
            => await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(24),
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });
    }
}
