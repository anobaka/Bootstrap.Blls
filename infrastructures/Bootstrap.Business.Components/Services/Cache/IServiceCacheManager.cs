using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrap.Business.Components.Services.Cache
{
    public interface IServiceCacheManager
    {
        Task<T> Get<T>(object id) where T : class;
        Task<Dictionary<object, T>> Get<T>(List<object> ids) where T : class;
        Task Set<T>(object id, T data) where T : class;
        Task Set<T>(IDictionary<object, T> data) where T : class;
        Task Delete<T>(object id) where T : class;
        Task Delete<T>(IEnumerable<object> ids) where T : class;
        Task<T> GetCustomCache<T>(string cacheKey) where T : class;
        Task SetCustomCache<T>(string cacheKey, T obj) where T : class;
    }
}