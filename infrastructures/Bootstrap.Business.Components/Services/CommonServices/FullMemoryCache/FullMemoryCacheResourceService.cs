using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.Services.Infrastructures;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Services.CommonServices.FullMemoryCache
{
    public abstract class
        FullMemoryCacheResourceService<TDbContext, TResource, TKey>
        where TDbContext : DbContext where TResource : class
    {
        protected FullMemoryCacheResourceVault<TKey, TResource> CacheVault;
        protected ResourceService<TDbContext, TResource, TKey> ResourceService;

        protected FullMemoryCacheResourceService(IServiceProvider serviceProvider)
        {
            ResourceService = new ResourceService<TDbContext, TResource, TKey>(serviceProvider);
            var data = ResourceService.GetAll().ConfigureAwait(false).GetAwaiter().GetResult()
                .ToDictionary(FuncExtensions.BuildKeySelector<TResource, TKey>(), t => t);
            CacheVault = new FullMemoryCacheResourceVault<TKey, TResource>(data);
        }

        public Task<TResource> GetByKey(TKey key) => Task.FromResult(CacheVault[key]);

        public Task<List<TResource>> GetByKeys(IEnumerable<TKey> keys)
        {
            var list = new List<TResource>();
            foreach (var k in keys)
            {
                if (CacheVault.TryGetValue(k, out var v))
                {
                    list.Add(v);
                }
            }

            return Task.FromResult(list);
        }

        public Task<TResource> GetFirst(Expression<Func<TResource, bool>> selector,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false)
        {
            var data = CacheVault.Values.Where(selector.Compile());
            if (orderBy != null)
            {
                var ob = orderBy.Compile();
                data = asc ? data.OrderBy(ob) : data.OrderByDescending(ob);
            }

            return Task.FromResult(data.FirstOrDefault());
        }

        public Task<List<TResource>> GetAll(Expression<Func<TResource, bool>> selector = null) =>
            Task.FromResult(
                (selector == null ? CacheVault.Values : CacheVault.Values.Where(selector.Compile())).ToList());

        public Task<SearchResponse<TResource>> Search(Func<TResource, bool> selector,
            int pageIndex, int pageSize, Func<TResource, object> orderBy = null, bool asc = false)
        {
            var resources = CacheVault.Values.ToList();
            if (selector != null)
            {
                resources = resources.Where(selector).ToList();
            }

            if (orderBy != null)
            {
                resources = (asc ? resources.OrderBy(orderBy) : resources.OrderByDescending(orderBy)).ToList();
            }

            var count = resources.Count;
            var data = resources.Skip(Math.Max(pageIndex - 1, 0) * pageSize).Take(pageSize).ToList();
            var result = new SearchResponse<TResource>(data, count, pageIndex, pageSize);
            return Task.FromResult(result);
        }

        public Task<BaseResponse> RemoveAll(Expression<Func<TResource, bool>> selector)
        {
            CacheVault.DeleteAll(selector.Compile());
            return ResourceService.RemoveAll(selector);
        }

        public Task<BaseResponse> RemoveByKey(TKey key)
        {
            CacheVault.RemoveByKey(key);
            return ResourceService.RemoveByKey(key);
        }

        public Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> keys)
        {
            var ks = keys.ToList();
            CacheVault.RemoveByKeys(ks);
            return ResourceService.RemoveByKeys(ks);
        }

        public async Task<SingletonResponse<TResource>> Create(TResource resource)
        {
            var rsp = await ResourceService.Create(resource);
            if (rsp.Data != null)
            {
                CacheVault.Add(rsp.Data);
            }

            return rsp;
        }

        public async Task<ListResponse<TResource>> Create(List<TResource> resources)
        {
            var rsp = await ResourceService.Create(resources);
            CacheVault.Add(rsp.Data);
            return rsp;
        }

        public int Count(Func<TResource, bool> selector)
        {
            return CacheVault.Values.Count(selector);
        }
    }
}