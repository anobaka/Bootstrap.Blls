using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.ResponseBuilders;
using Bootstrap.Business.Components.Services.Options;
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
    public class BaseService<TDbContext> : ServiceBase<TDbContext> where TDbContext : DbContext
    {
        public BaseService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        #region IdRelatedOperations

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetByKey<TResource>(object key)
            where TResource : class
        {
            var exp = ExpressionExtensions.BuildKeyEqualsExpression<TResource>(key);
            return await GetFirst(exp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetByKeys<TResource>(IEnumerable<object> keys)
            where TResource : class
        {
            var exp = ExpressionExtensions.BuildKeyContainsExpression<TResource>(keys.Distinct().ToList());
            var resources = (await GetAll(exp)).ToDictionary(FuncExtensions.BuildKeySelector<TResource>(), t => t);
            return resources.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveByKey<TResource>(object key) where TResource : class
        {
            var exp = ExpressionExtensions.BuildKeyEqualsExpression<TResource>(key);
            var rsp = await RemoveAll(exp);
            return rsp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveByKeys<TResource>(IEnumerable<object> keys)
            where TResource : class
        {
            var ks = keys.ToList();
            var exp = ExpressionExtensions.BuildKeyContainsExpression<TResource>(ks);
            var rsp = await RemoveAll(exp);
            return rsp;
        }

        public virtual async Task<SingletonResponse<TResource>> UpdateByKey<TResource>(object key,
            Action<TResource> modify)
            where TResource : class
        {
            var r = await GetByKey<TResource>(key);
            modify(r);
            await DbContext.SaveChangesAsync();
            return new SingletonResponse<TResource>(r);
        }

        public virtual async Task<ListResponse<TResource>> UpdateByKeys<TResource>(IReadOnlyCollection<object> keys,
            Action<TResource> modify)
            where TResource : class
        {
            var rs = await GetByKeys<TResource>(keys);
            foreach (var r in rs)
            {
                modify(r);
            }

            await DbContext.SaveChangesAsync();
            return new ListResponse<TResource>(rs);
        }

        #endregion

        #region IdUnrelatedOperations

        /// <summary>
        /// 获取第一条
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetFirst<TResource>(Expression<Func<TResource, bool>> selector,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false)
            where TResource : class
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

            var result = await query.FirstOrDefaultAsync();
            return result;
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector">Null for getting all resources.</param>
        /// <param name="useNewDbContext"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetAll<TResource>(Expression<Func<TResource, bool>> selector,
            bool useNewDbContext = false)
            where TResource : class
        {
            var dbContext = useNewDbContext ? NewScopeDbContext : DbContext;
            IQueryable<TResource> query = dbContext.Set<TResource>();
            if (selector != null)
            {
                query = query.Where(selector);
            }

            var result = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public virtual async Task<SearchResponse<TResource>> Search<TResource>(
            Expression<Func<TResource, bool>> selector, int pageIndex, int pageSize,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false,
            Expression<Func<TResource, object>> include = null)
            where TResource : class
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
            var result = new SearchResponse<TResource>(data, count, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveAll<TResource>(Expression<Func<TResource, bool>> selector)
            where TResource : class
        {
            DbContext.RemoveRange(DbContext.Set<TResource>().Where(selector));
            return BaseResponseBuilder.Build(await DbContext.SaveChangesAsync() > 0
                ? ResponseCode.Success
                : ResponseCode.NotModified);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<SingletonResponse<TResource>> Add<TResource>(TResource resource)
            where TResource : class
        {
            DbContext.Add(resource);
            await DbContext.SaveChangesAsync();
            return new SingletonResponse<TResource>(resource);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="resources"></param>
        /// <returns></returns>
        public virtual async Task<ListResponse<TResource>> AddRange<TResource>(List<TResource> resources)
            where TResource : class
        {
            DbContext.AddRange(resources);
            await DbContext.SaveChangesAsync();
            return new ListResponse<TResource>(resources);
        }

        public virtual async Task<int> Count<TResource>(Expression<Func<TResource, bool>> selector)
            where TResource : class
        {
            return await DbContext.Set<TResource>().CountAsync(selector);
        }

        public virtual async Task<BaseResponse> Update<TResource>(TResource resource)
        {
            DbContext.Entry(resource).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return BaseResponseBuilder.Ok;
        }

        public virtual async Task<SingletonResponse<TResource>> UpdateFirst<TResource>(
            Expression<Func<TResource, bool>> selector,
            Action<TResource> modify)
            where TResource : class
        {
            var r = (await GetAll(selector)).FirstOrDefault();
            modify(r);
            await DbContext.SaveChangesAsync();
            return new SingletonResponse<TResource>(r);
        }

        public virtual async Task<ListResponse<TResource>> UpdateAll<TResource>(
            Expression<Func<TResource, bool>> selector,
            Action<TResource> modify)
            where TResource : class
        {
            var rs = await GetAll(selector);
            foreach (var r in rs)
            {
                modify(r);
            }

            await DbContext.SaveChangesAsync();
            return new ListResponse<TResource>(rs);
        }

        #endregion
    }
}