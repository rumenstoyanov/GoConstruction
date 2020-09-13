using GoApi.Services.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoApi.Services.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public Task SetCacheValueAsync<T>(string key, T value) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> TryGetCacheValueAsync<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
