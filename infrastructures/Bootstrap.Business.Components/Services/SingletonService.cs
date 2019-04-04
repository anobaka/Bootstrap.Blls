using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.Services.Cache;
using Bootstrap.Business.Components.Services.Options;
using Bootstrap.Business.Extensions.ResponseBuilders;
using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Bootstrap.Business.Components.Services
{
    public abstract class SingletonService<TDbContext> where TDbContext : DbContext
    {
        protected IDistributedCache DistributedCache => GetRequiredService<IDistributedCache>();
        protected IServiceProvider ServiceProvider;
        protected CacheRedisClient Redis => GetRequiredService<CacheRedisClient>();
        protected IServiceCacheManager CacheManager;
        protected IOptions<ServiceOptions> Options;
        protected ILogger Logger;

        private static readonly ConcurrentDictionary<string, object> CachedServices =
            new ConcurrentDictionary<string, object>();

        /// <summary>
        /// {Type Fullname}-All
        /// </summary>
        private const string AllDataCacheKeyTemplate = "{0}-All";

        private class CacheData<T>
        {
            public CacheData()
            {
            }

            public CacheData(object key, T data)
            {
                Key = key.ToString();
                Data = data;
            }

            public CacheData(T data, Func<T, object> keySelector)
            {
                Data = data;
                Key = keySelector(data).ToString();
            }

            public string Key { get; }
            public T Data { get; }
        }

        protected SingletonService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            Options = serviceProvider.GetRequiredService<IOptions<ServiceOptions>>();
            if (Options.Value.EnableCache)
            {
                CacheManager = serviceProvider.GetService<IServiceCacheManager>() ?? new MemoryServiceCacheManager();
            }
        }

        protected virtual TDbContext DbContext => GetRequiredService<TDbContext>();

        protected virtual TDbContext DbContextFromNewScope => GetRequiredServiceFromNewScope<TDbContext>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">Should not be true while getting a scoped service.</param>
        /// <returns></returns>
        protected virtual T GetRequiredService<T>(bool cache = false)
        {
            object service = null;
            if (cache)
            {
                CachedServices.TryGetValue(SpecificTypeUtils<T>.Type.FullName ?? throw new InvalidOperationException(),
                    out service);
            }

            if (service == null)
            {
                service = (ServiceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.RequestServices ??
                           ServiceProvider).GetRequiredService<T>();
                if (cache)
                {
                    CachedServices.TryAdd(SpecificTypeUtils<T>.Type.FullName, service);
                }
            }

            return (T) service;
        }

        protected virtual T GetRequiredServiceFromNewScope<T>() =>
            ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<T>();

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetByKey<TResource>(object key)
            where TResource : class
        {
            TResource resource = null;
            var dataNotExist = false;
            if (Options.Value.EnableCache)
            {
                resource = await CacheManager.Get<TResource>(key);
                dataNotExist = resource == null;
//                await Redis.ExecuteAsync(async t =>
//                {
//                    var json = await t.HashGetAsync(SpecificTypeUtils<TResource>.Type.FullName, key.ToString());
//                    if (!string.IsNullOrEmpty(json))
//                    {
//                        var cache = JsonConvert.DeserializeObject<CacheData<TResource>>(json);
//                        if (cache.Data == null)
//                        {
//                            dataNotExist = true;
//                        }
//                        else
//                        {
//                            resource = cache.Data;
//                        }
//                    }
//                });
            }

            if (!dataNotExist && resource == null)
            {
                resource = await GetFirst<TResource>(t =>
                    DynamicCacheOptions<TResource>.KeySelector(t).Equals(key));
                if (Options.Value.EnableCache)
                {
                    await CacheManager.Set(key, resource);
//                    await Redis.ExecuteAsync(async t =>
//                    {
//                        await t.HashSetAsync(SpecificTypeUtils<TResource>.Type.FullName, key.ToString(),
//                            JsonConvert.SerializeObject(new CacheData<TResource>(key, resource)));
//                    });
                }
            }

            return resource;
        }

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetByKeys<TResource>(IEnumerable<object> keys)
            where TResource : class
        {
            var ks = keys.Distinct().ToList();
            List<TResource> resources = null;
            var tobeQueriedDataKeys = ks.ToList();
            if (Options.Value.EnableCache)
            {
//                Dictionary<string, TResource> cache = null;
                var cache = await CacheManager.Get<TResource>(ks);
//                await Redis.ExecuteAsync(async t =>
//                {
//                    var jsons = await t.HashGetAsync(SpecificTypeUtils<TResource>.Type.FullName,
//                        ks.Select(a => (RedisValue) a.ToString()).ToArray());
//                    cache = jsons?.Where(a => !string.IsNullOrEmpty(a)).Select(a =>
//                            JsonConvert.DeserializeObject<CacheData<TResource>>(a))
//                        .ToDictionary(a => a.Key, a => a.Data);
//                });
                if (cache != null)
                {
                    tobeQueriedDataKeys.RemoveAll(t => cache.ContainsKey(t.ToString()));
                    resources = cache.Where(a => a.Value != null).Select(a => a.Value).ToList();
                }
            }

            if (tobeQueriedDataKeys.Any())
            {
                var tResources =
                    (await GetAll<TResource>(t =>
                        tobeQueriedDataKeys.Contains(DynamicCacheOptions<TResource>.KeySelector(t))))
                    .ToDictionary(t => DynamicCacheOptions<TResource>.KeySelector(t), t => t);
                if (tResources.Any())
                {
                    if (resources == null)
                    {
                        resources = new List<TResource>();
                    }

                    resources.AddRange(tResources.Values);
                }

                if (Options.Value.EnableCache)
                {
                    await CacheManager.Set(tResources);
//                    await Redis.ExecuteAsync(async t =>
//                    {
//                        await t.HashSetAsync(SpecificTypeUtils<TResource>.Type.FullName,
//                            tobeQueriedDataKeys.Select(a =>
//                            {
//                                tResources.TryGetValue(a, out var resource);
//                                return new HashEntry(a.ToString(),
//                                    JsonConvert.SerializeObject(new CacheData<TResource>(a, resource)));
//                            }).ToArray());
//                    });
                }
            }

            return resources ?? new List<TResource>();
        }

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> DeleteByKey<TResource>(object key) where TResource : class
        {
            var rsp = await Delete<TResource>(t => DynamicCacheOptions<TResource>.KeySelector(t).Equals(key));

            if (Options.Value.EnableCache)
            {
                await CacheManager.Delete<TResource>(key);
//                await Redis.ExecuteAsync(t =>
//                    t.HashDeleteAsync(SpecificTypeUtils<TResource>.Type.FullName, key.ToString()));
            }

            return rsp;
        }

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> DeleteByKeys<TResource>(IEnumerable<object> keys)
            where TResource : class
        {
            var rsp = await Delete<TResource>(t => keys.Contains(DynamicCacheOptions<TResource>.KeySelector(t)));

            if (Options.Value.EnableCache)
            {
//                await Redis.ExecuteAsync(t =>
//                    t.HashDeleteAsync(SpecificTypeUtils<TResource>.Type.FullName,
//                        keys.Select(a => (RedisValue) a.ToString()).ToArray()));
                await CacheManager.Delete<TResource>(keys.ToList());
            }

            return rsp;
        }

        /// <summary>
        /// 获取第一条，静态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetFirst<TResource>(Expression<Func<TResource, bool>> selector,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false, CacheOptions cacheOptions = null)
            where TResource : class
        {
            TResource result = null;
            if (cacheOptions?.IsValid == true)
            {
                result = await DistributedCache.GetObjectAsync<TResource>(cacheOptions.CacheKey);
            }

            if (result == null)
            {
                IQueryable<TResource> query = DbContext.Set<TResource>();
                if (selector != null)
                {
                    query = query.Where(selector);
                }

                if (orderBy != null)
                {
                    query = asc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                }

                result = await query.FirstOrDefaultAsync();
                if (cacheOptions?.IsValid == true && result != null)
                {
                    await DistributedCache.SetObjectAsync(cacheOptions.CacheKey, result);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取全部，静态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector">为空则获取全部，如果使用缓存，会自动指定缓存key</param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetAll<TResource>(Expression<Func<TResource, bool>> selector,
            CacheOptions cacheOptions = null)
            where TResource : class
        {
            List<TResource> result = null;
            if (selector == null && cacheOptions != null)
            {
                cacheOptions.CacheKey = string.Format(AllDataCacheKeyTemplate,
                    SpecificTypeUtils<TResource>.Type.FullName);
            }

            if (cacheOptions?.IsValid == true)
            {
                if (!cacheOptions.Refresh)
                {
                    result = await DistributedCache.GetObjectAsync<List<TResource>>(cacheOptions.CacheKey);
                }
            }

            if (result == null)
            {
                IQueryable<TResource> query = DbContext.Set<TResource>();
                if (selector != null)
                {
                    query = query.Where(selector);
                }

                result = await query.ToListAsync();
                if (cacheOptions?.IsValid == true && result != null)
                {
                    await DistributedCache.SetObjectAsync(cacheOptions.CacheKey, result);
                }
            }

            return result ?? new List<TResource>();
        }

        /// <summary>
        /// 搜索，静态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="cacheOptions"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public virtual async Task<SearchResponse<TResource>> Search<TResource>(
            Expression<Func<TResource, bool>> selector, int pageIndex, int pageSize,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false, CacheOptions cacheOptions = null,
            Expression<Func<TResource, object>> include = null)
            where TResource : class
        {
            SearchResponse<TResource> result = null;
            if (cacheOptions?.IsValid == true)
            {
                result = await DistributedCache.GetObjectAsync<SearchResponse<TResource>>(cacheOptions.CacheKey);
            }

            if (result == null)
            {
                IQueryable<TResource> query = DbContext.Set<TResource>();
                if (selector != null)
                {
                    query = include == null ? query.Where(selector) : query.Include(include).Where(selector);
                }

                if (orderBy != null)
                {
                    query = asc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                }

                var count = await query.CountAsync();
                var data = await query.Skip(Math.Max(pageIndex - 1, 0) * pageSize).Take(pageSize).ToListAsync();
                result = new SearchResponse<TResource>(data, count, pageIndex, pageSize);
                if (cacheOptions?.IsValid == true)
                {
                    await DistributedCache.SetObjectAsync(cacheOptions.CacheKey, result);
                }
            }

            if (result.Data == null)
            {
                result.Data = new List<TResource>();
            }

            return result;
        }

        /// <summary>
        /// 删除，无缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> Delete<TResource>(Expression<Func<TResource, bool>> selector)
            where TResource : class
        {
            DbContext.RemoveRange(DbContext.Set<TResource>().Where(selector));
            return BaseResponseBuilder.Build(await DbContext.SaveChangesAsync() > 0
                ? ResponseCode.Success
                : ResponseCode.NotModified);
        }

        /// <summary>
        /// 创建，动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<SingletonResponse<TResource>> Create<TResource>(TResource resource)
            where TResource : class
        {
            DbContext.Add(resource);
            await DbContext.SaveChangesAsync();
            if (Options.Value.EnableCache)
            {
                await Redis.ExecuteAsync(async t =>
                {
                    await t.HashDeleteAsync(SpecificTypeUtils<TResource>.Type.FullName,
                        DynamicCacheOptions<TResource>.KeySelector(resource).ToString());
                });
            }

            return new SingletonResponse<TResource>(resource);
        }

        /// <summary>
        /// 创建，动态缓存
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resources"></param>
        /// <returns></returns>
        public virtual async Task<ListResponse<TResource>> Create<TResource>(List<TResource> resources)
            where TResource : class
        {
            DbContext.AddRange(resources);
            await DbContext.SaveChangesAsync();
            if (Options.Value.EnableCache)
            {
                await Redis.ExecuteAsync(async t =>
                {
                    await t.HashDeleteAsync(SpecificTypeUtils<TResource>.Type.FullName,
                        resources.Select(a => (RedisValue) DynamicCacheOptions<TResource>.KeySelector(a).ToString())
                            .ToArray());
                });
            }

            return new ListResponse<TResource>(resources);
        }
    }

    public abstract class SingletonService<TDbContext, TDefaultResource> : SingletonService<TDbContext>
        where TDbContext : DbContext where TDefaultResource : class
    {
        protected SingletonService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<TDefaultResource> GetByKey(object key) => await GetByKey<TDefaultResource>(key);

        /// <summary>
        /// 动态缓存
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<List<TDefaultResource>> GetByKeys(IEnumerable<object> keys) =>
            await GetByKeys<TDefaultResource>(keys);

        /// <summary>
        /// 获取单条默认资源，静态缓存
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        public virtual async Task<TDefaultResource> GetFirst(Expression<Func<TDefaultResource, bool>> selector,
            Expression<Func<TDefaultResource, object>> orderBy = null,
            bool asc = true, CacheOptions cacheOptions = null) =>
            await GetFirst<TDefaultResource>(selector, orderBy, asc, cacheOptions);

        /// <summary>
        /// 获取全部默认资源，静态缓存，会自动指定缓存key
        /// </summary>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        public virtual async Task<List<TDefaultResource>> GetAll(CacheOptions cacheOptions = null) =>
            await GetAll<TDefaultResource>(null, cacheOptions);

        /// <summary>
        /// 获取全部默认资源，静态缓存
        /// </summary>
        /// <param name="selector">为空则获取全部，如果使用了缓存会自动指定缓存key</param>
        /// <param name="cacheOptions"></param>
        /// <returns></returns>
        public virtual async Task<List<TDefaultResource>> GetAll(Expression<Func<TDefaultResource, bool>> selector,
            CacheOptions cacheOptions = null) =>
            await GetAll<TDefaultResource>(selector, cacheOptions);

        /// <summary>
        /// 搜索默认资源，静态缓存
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="pageIndex">从1开始</param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="cacheOptions"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public virtual async Task<SearchResponse<TDefaultResource>> Search(
            Expression<Func<TDefaultResource, bool>> selector, int pageIndex, int pageSize,
            Expression<Func<TDefaultResource, object>> orderBy = null, bool asc = false,
            CacheOptions cacheOptions = null, Expression<Func<TDefaultResource, object>> include = null) =>
            await Search<TDefaultResource>(selector, pageIndex, pageSize, orderBy, asc, cacheOptions, include);

        /// <summary>
        /// 删除默认资源，动态缓存
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> Delete(Expression<Func<TDefaultResource, bool>> selector) =>
            await Delete<TDefaultResource>(selector);

        /// <summary>
        /// 删除，动态缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> DeleteByKey(object key)
        {
            return await base.DeleteByKey<TDefaultResource>(key);
        }

        /// <summary>
        /// 删除，动态缓存
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> DeleteByKeys(IEnumerable<object> keys)
        {
            return await base.DeleteByKeys<TDefaultResource>(keys);
        }

        /// <summary>
        /// 创建默认资源，动态缓存
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<SingletonResponse<TDefaultResource>> Create(TDefaultResource resource) =>
            await base.Create(resource);

        /// <summary>
        /// 创建默认资源，动态缓存
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public virtual async Task<ListResponse<TDefaultResource>> Create(List<TDefaultResource> resources) =>
            await base.Create(resources);
    }
}