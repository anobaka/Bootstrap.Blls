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
        /// <summary>
        /// Type - key - instance
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, object>> _cache =
            new ConcurrentDictionary<string, ConcurrentDictionary<object, object>>();

        public Task<T> Get<T>(object id) where T : class
        {
            T instance = null;
            if (_cache.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var v) && v.TryGetValue(id, out var i))
            {
                instance = (T) i;
            }

            return Task.FromResult(instance);
        }

        public Task<Dictionary<object, T>> Get<T>(List<object> ids) where T : class
        {
            Dictionary<object, T> result = null;
            if (_cache.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var v))
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
            var vault = _cache.GetOrAdd(SpecificTypeUtils<T>.Type.FullName,
                t => new ConcurrentDictionary<object, object>());
            vault[id] = data;
            return Task.CompletedTask;
        }

        public Task Set<T>(IDictionary<object, T> data) where T : class
        {
            var vault = _cache.GetOrAdd(SpecificTypeUtils<T>.Type.FullName,
                t => new ConcurrentDictionary<object, object>());
            foreach (var k in data)
            {
                vault[k.Key] = k.Value;
            }

            return Task.CompletedTask;
        }

        public Task Delete<T>(object id) where T : class
        {
            if (_cache.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var vault))
            {
                vault.TryRemove(id, out _);
            }

            return Task.CompletedTask;
        }

        public Task Delete<T>(List<object> ids) where T : class
        {
            if (_cache.TryGetValue(SpecificTypeUtils<T>.Type.FullName, out var vault))
            {
                ids.ForEach(t => vault.TryRemove(t, out _));
            }

            return Task.CompletedTask;
        }
    }
}