using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoApi.Services.Interfaces
{
    public interface ICacheService
    {
        Task SetCacheValueAsync<T>(string key, T value) where T : class;
        Task<T> TryGetCacheValueAsync<T>(string key) where T : class;
    }
}
