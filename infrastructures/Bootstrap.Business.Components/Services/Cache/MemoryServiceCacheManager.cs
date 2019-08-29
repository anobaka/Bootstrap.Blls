using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Extensions;

namespace Bootstrap.Business.Components.Services.Cache
{
    public class MemoryServiceCacheManager : IServiceCacheManager
    {
        #region Key based cache

        /// <summary>
        /// Type - Key - Data
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, object>> _defaultCacheVault =
            new ConcurrentDictionary<string, ConcurrentDictionary<object, object>>();

        public Task<T> Get<T>(object id) where T : class
        {
            T instance = null;
            if (_defaultCacheVault.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var v) &&
                v.TryGetValue(id, out var i))
            {
                instance = (T) i;
            }

            return Task.FromResult(instance);
        }

        public Task<Dictionary<object, T>> Get<T>(List<object> ids) where T : class
        {
            Dictionary<object, T> result = null;
            if (_defaultCacheVault.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var v))
            {
                foreach (var i in ids)
                {
                    if (v.TryGetValue(i, out var r))
                    {
                        if (result == null)
                        {
                            result = new Dictionary<object, T>();
                        }

                        result.Add(i, (T) r);
                    }
                }
            }

            return Task.FromResult(result);
        }

        public Task Set<T>(object id, T data) where T : class
        {
            var vault = _defaultCacheVault.GetOrAdd(SpecificTypeUtils<T>.Type.FullName,
                t => new ConcurrentDictionary<object, object>());
            vault[id] = data;
            return Task.CompletedTask;
        }

        public Task Set<T>(IDictionary<object, T> data) where T : class
        {
            var vault = _defaultCacheVault.GetOrAdd(SpecificTypeUtils<T>.Type.FullName,
                t => new ConcurrentDictionary<object, object>());
            foreach (var k in data)
            {
                vault[k.Key] = k.Value;
            }

            return Task.CompletedTask;
        }

        public Task Delete<T>(object id) where T : class
        {
            if (_defaultCacheVault.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var vault))
            {
                vault.TryRemove(id, out _);
            }

            return Task.CompletedTask;
        }

        public Task Delete<T>(IEnumerable<object> ids) where T : class
        {
            if (_defaultCacheVault.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var vault))
            {
                foreach (var i in ids)
                {
                    vault.TryRemove(i, out _);
                }
            }

            return Task.CompletedTask;
        }

        public Task Delete<T>(Func<T, bool> selector) where T : class
        {
            if (_defaultCacheVault.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var vault))
            {
                var keys = vault.Where(t => selector((T) t.Value)).Select(a => a.Key);
                foreach (var k in keys)
                {
                    vault.Remove(k, out _);
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Custom cache key based cache

        /// <summary>
        /// CacheKey - Data
        /// </summary>
        private readonly ConcurrentDictionary<string, object> _customCacheVault =
            new ConcurrentDictionary<string, object>();

        public Task<T> GetCustomKeyCache<T>(string cacheKey) where T : class
        {
            return _customCacheVault.TryGetValue(_buildCacheKey<T>(cacheKey), out var o)
                ? Task.FromResult((T) o)
                : null;
        }

        public Task SetCustomKeyCache<T>(string cacheKey, T obj) where T : class
        {
            _customCacheVault[_buildCacheKey<T>(cacheKey)] = obj;
            return Task.CompletedTask;
        }

        public Task DeleteCustomKeyCache<T>(string cacheKey) where T : class
        {
            _customCacheVault.Remove(cacheKey, out _);
            return Task.CompletedTask;
        }

        private string _buildCacheKey<T>(string key)
        {
            return $"{SpecificTypeUtils<T>.Type.FullName}-{key}";
        }

        #endregion
    }
}