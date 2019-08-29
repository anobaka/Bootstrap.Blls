using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrap.Business.Components.Services.Cache
{
    public interface IServiceCacheManager
    {
        #region Key based cache

        Task<T> Get<T>(object id) where T : class;
        Task<Dictionary<object, T>> Get<T>(List<object> ids) where T : class;
        Task Set<T>(object id, T data) where T : class;
        Task Set<T>(IDictionary<object, T> data) where T : class;
        Task Delete<T>(object id) where T : class;
        Task Delete<T>(IEnumerable<object> ids) where T : class;
        Task Delete<T>(Func<T, bool> selector) where T : class;

        #endregion

        #region Custom cache key based cache

        Task<T> GetCustomKeyCache<T>(string cacheKey) where T : class;
        Task SetCustomKeyCache<T>(string cacheKey, T obj) where T : class;
        Task DeleteCustomKeyCache<T>(string cacheKey) where T : class;

        #endregion
    }
}