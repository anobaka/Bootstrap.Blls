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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Bootstrap.Business.Components.Services.Infrastructures
{
    public abstract class AbstractService<TDbContext> where TDbContext : DbContext
    {
        protected IServiceProvider ServiceProvider;
        protected IServiceCacheManager CacheManager;
        protected IOptions<ServiceOptions> Options;
        protected ILogger Logger;

        private static readonly ConcurrentDictionary<string, object> CachedServices =
            new ConcurrentDictionary<string, object>();

        protected virtual TDbContext DbContext => GetRequiredService<TDbContext>();
        protected virtual TDbContext DbContextFromNewScope => GetRequiredServiceFromNewScope<TDbContext>();

        protected virtual IDbContextTransaction GetTransaction(out bool isNewTransaction, bool ignoreExisted = false)
        {
            isNewTransaction = ignoreExisted || DbContext.Database.CurrentTransaction == null;
            return isNewTransaction ? DbContext.Database.BeginTransaction() : DbContext.Database.CurrentTransaction;
        }

        protected AbstractService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            Options = serviceProvider.GetRequiredService<IOptions<ServiceOptions>>();
            if (Options.Value.EnableCache)
            {
                CacheManager = serviceProvider.GetService<IServiceCacheManager>() ?? new MemoryServiceCacheManager();
            }
        }

        #region Services

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

        #endregion

        #region Helpers

        /// <summary>
        /// Temporary method.
        /// todo: optimize
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="v"></param>
        /// <param name="buildBody">key type, key, instance value(s), body</param>
        /// <returns></returns>
        private Expression<Func<TResource, TOut>> _buildKeyExpression<TResource, TOut>(object v,
            Func<Type, Expression, Expression, Expression> buildBody)
        {
            var type = SpecificTypeUtils<TResource>.Type;
            var keyProperty = type.GetKeyProperty();
            if (keyProperty == null)
            {
                throw new InvalidOperationException($"Can not find a key property of type: {type.FullName}");
            }

            var arg = Expression.Parameter(type);
            var key = Expression.Property(arg, keyProperty);
            var value = v == null ? null : Expression.Constant(v);
            var body = buildBody(keyProperty.PropertyType, key, value);
            var lambda = Expression.Lambda<Func<TResource, TOut>>(body, arg);
            return lambda;
        }

        private Func<TResource, object> _getKeySelector<TResource>()
        {
            return _buildKeyExpression<TResource, object>(null,
                (t, key, value) => Expression.TypeAs(key, SpecificTypeUtils<object>.Type)).Compile();
        }

        private Expression<Func<TResource, bool>> _buildKeyEqualsExpression<TResource>(object key)
        {
            return _buildKeyExpression<TResource, bool>(key, (type, i, v) => Expression.Equal(i, v));
        }

        private Expression<Func<TResource, bool>> _buildKeyContainsExpression<TResource>(List<object> keys)
        {
            var type = SpecificTypeUtils<TResource>.Type;
            var keyProperty = type.GetKeyProperty();
            if (keyProperty == null)
            {
                throw new InvalidOperationException($"Can not find a key property of type: {type.FullName}");
            }

            var trueTypeKeys = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))
                ?.MakeGenericMethod(keyProperty.PropertyType)
                .Invoke(null, new object[] {keys});
            return _buildKeyExpression<TResource, bool>(trueTypeKeys,
                (t, i, v) => Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] {t}, v, i));
        }

        #endregion

        #region IdRelatedOperations

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
            }

            if (!dataNotExist && resource == null)
            {
                var exp = _buildKeyEqualsExpression<TResource>(key);
                resource = await GetFirst(exp);
                if (Options.Value.EnableCache)
                {
                    await CacheManager.Set(key, resource);
                }
            }

            return resource;
        }

        #region IdUnrelatedOperations

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
                var cache = await CacheManager.Get<TResource>(ks);
                if (cache != null)
                {
                    tobeQueriedDataKeys.RemoveAll(t => cache.ContainsKey(t.ToString()));
                    resources = cache.Where(a => a.Value != null).Select(a => a.Value).ToList();
                }
            }

            if (tobeQueriedDataKeys.Any())
            {
                var exp = _buildKeyContainsExpression<TResource>(tobeQueriedDataKeys);
                var tResources = (await GetAll(exp)).ToDictionary(_getKeySelector<TResource>(), t => t);
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
            var exp = _buildKeyEqualsExpression<TResource>(key);
            var rsp = await Delete(exp);

            if (Options.Value.EnableCache)
            {
                await CacheManager.Delete<TResource>(key);
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
            var ks = keys.ToList();
            var exp = _buildKeyContainsExpression<TResource>(ks);
            var rsp = await Delete(exp);

            if (Options.Value.EnableCache)
            {
                await CacheManager.Delete<TResource>(ks);
            }

            return rsp;
        }

        #endregion

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
                result = await CacheManager.GetCustomCache<TResource>(cacheOptions.CacheKey);
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
                    await CacheManager.SetCustomCache(cacheOptions.CacheKey, result);
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
        /// <param name="useNewDbContext"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetAll<TResource>(Expression<Func<TResource, bool>> selector,
            CacheOptions cacheOptions = null, bool useNewDbContext = false)
            where TResource : class
        {
            List<TResource> result = null;
            if (selector == null && cacheOptions != null)
            {
                result = await CacheManager.GetCustomCache<List<TResource>>(cacheOptions.CacheKey);
            }

            if (cacheOptions?.IsValid == true)
            {
                if (!cacheOptions.Refresh)
                {
                    result = await CacheManager.GetCustomCache<List<TResource>>(cacheOptions.CacheKey);
                }
            }

            if (result == null)
            {
                var dbContext = useNewDbContext ? DbContextFromNewScope : DbContext;
                IQueryable<TResource> query = dbContext.Set<TResource>();
                if (selector != null)
                {
                    query = query.Where(selector);
                }

                result = await query.ToListAsync();
                if (cacheOptions?.IsValid == true && result != null)
                {
                    await CacheManager.SetCustomCache(cacheOptions.CacheKey, result);
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
                result = await CacheManager.GetCustomCache<SearchResponse<TResource>>(cacheOptions.CacheKey);
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
                    await CacheManager.SetCustomCache(cacheOptions.CacheKey, result);
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
            return new ListResponse<TResource>(resources);
        }

        #endregion
    }

    public abstract class AbstractService<TDbContext, TDefaultResource> : AbstractService<TDbContext>
        where TDbContext : DbContext where TDefaultResource : class
    {
        protected AbstractService(IServiceProvider serviceProvider) : base(serviceProvider)
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
            bool asc = false, CacheOptions cacheOptions = null) =>
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
        /// <param name="useNewDbContext"></param>
        /// <returns></returns>
        public virtual async Task<List<TDefaultResource>> GetAll(Expression<Func<TDefaultResource, bool>> selector,
            CacheOptions cacheOptions = null, bool useNewDbContext = false) =>
            await GetAll<TDefaultResource>(selector, cacheOptions, useNewDbContext);

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