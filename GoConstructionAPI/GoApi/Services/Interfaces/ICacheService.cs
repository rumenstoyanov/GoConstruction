using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Only for use by external consumers, not used interally in service.
        /// </summary>
        string BuildCacheKeyFromUrl(string url, Guid oid);
        Task SetCacheValueAsync<T>(string key, T value) where T : class;
        Task SetCacheValueAsync<T>(HttpRequest request, Guid oid, T value) where T : class;
        /// <summary>
        /// The TTL in seconds is specified in configuration, bound to RedisSettings at startup and consumed in implementations of this interface.
        /// </summary>
        Task SetCacheValueWithExpiryAsync<T>(string key, T value) where T : class;
        Task SetCacheValueWithExpiryAsync<T>(HttpRequest request, Guid oid, T value) where T : class;
        Task<T> TryGetCacheValueAsync<T>(string key) where T : class;
        Task<T> TryGetCacheValueAsync<T>(HttpRequest request, Guid oid) where T : class;
        Task TryDeleteCacheValueAsync(string key);
        Task TryDeleteCacheValueAsync(HttpRequest request, Guid oid);

    }
}
