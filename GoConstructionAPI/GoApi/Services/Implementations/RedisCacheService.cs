using GoApi.Services.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using GoApi.Data.Constants;

namespace GoApi.Services.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly bool _isEnabled;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, RedisSettings redisSettings)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _isEnabled = redisSettings.IsEnabled;
        }

        private string BuildCacheKeyFromRequest(HttpRequest request, Guid oid)
        {
            return $"{oid}|{request.Path}";
        }

        public async Task SetCacheValueAsync<T>(string key, T value) where T : class
        {
            if (!_isEnabled)
            {
                return;
            }
            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync(key, JsonConvert.SerializeObject(value));
        }
        public async Task SetCacheValueAsync<T>(HttpRequest request, Guid oid, T value) where T : class
        {
            await SetCacheValueAsync(BuildCacheKeyFromRequest(request, oid), value);
        }

        public async Task<T> TryGetCacheValueAsync<T>(string key) where T : class
        {
            if (!_isEnabled)
            {
                return null;
            }
            var db = _connectionMultiplexer.GetDatabase();
            RedisValue value = await db.StringGetAsync(key);

            if (value.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return null;
        }

        public async Task<T> TryGetCacheValueAsync<T>(HttpRequest request, Guid oid) where T : class
        {
            return await TryGetCacheValueAsync<T>(BuildCacheKeyFromRequest(request, oid));
        }

        public async Task TryDeleteCacheValueAsync(string key)
        {
            if (!_isEnabled)
            {
                return;
            }
            var db = _connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        public async Task TryDeleteCacheValueAsync(HttpRequest request, Guid oid)
        {
            await TryDeleteCacheValueAsync(BuildCacheKeyFromRequest(request, oid));
        }
    }
}
