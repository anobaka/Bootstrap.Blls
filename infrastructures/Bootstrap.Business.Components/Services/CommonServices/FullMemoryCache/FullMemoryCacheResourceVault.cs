using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Extensions;

namespace Bootstrap.Business.Components.Services.CommonServices.FullMemoryCache
{
    public class FullMemoryCacheResourceVault<TKey, TResource> where TResource : class
    {
        private readonly ConcurrentDictionary<TKey, TResource> _defaultCacheVault;

        public FullMemoryCacheResourceVault(Dictionary<TKey, TResource> dict)
        {
            _defaultCacheVault = new ConcurrentDictionary<TKey, TResource>(dict);
        }

        public TResource this[TKey key] => _defaultCacheVault.TryGetValue(key, out var v) ? v : null;

        public bool TryGetValue(TKey key, out TResource v) => _defaultCacheVault.TryGetValue(key, out v);

        public ICollection<TResource> Values => _defaultCacheVault.Values;

        public void DeleteAll(Func<TResource, bool> selector)
        {
            var keys = _defaultCacheVault.Where(t => selector(t.Value)).Select(a => a.Key);
            foreach (var k in keys)
            {
                _defaultCacheVault.Remove(k, out _);
            }
        }

        public void RemoveByKey(TKey key) => _defaultCacheVault.Remove(key, out _);

        public void RemoveByKeys(IEnumerable<TKey> keys)
        {
            foreach (var k in keys)
            {
                RemoveByKey(k);
            }
        }

        public void Add(TResource data)
        {
            _defaultCacheVault[data.GetKeyPropertyValue<TKey>()] = data;
        }

        public void Add(IEnumerable<TResource> data)
        {
            if (data != null)
            {
                foreach (var d in data)
                {
                    Add(d);
                }
            }
        }
    }
}