using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bootstrap.Business.Components.Services.Infrastructures
{
    public class ResourceService<TDbContext, TResource, TKey> : ServiceBase<TDbContext>
        where TDbContext : DbContext where TResource : class
    {
        protected BaseService<TDbContext> BaseService;

        public ResourceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            BaseService = serviceProvider.GetRequiredService<BaseService<TDbContext>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetByKey(TKey key) =>
            await BaseService.GetByKey<TResource>(key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetByKeys(IEnumerable<TKey> keys) =>
            await BaseService.GetByKeys<TResource>(keys.Cast<object>());

        /// <summary>
        /// 获取单条默认资源
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public virtual async Task<TResource> GetFirst(Expression<Func<TResource, bool>> selector,
            Expression<Func<TResource, object>> orderBy = null,
            bool asc = false) =>
            await BaseService.GetFirst(selector, orderBy, asc);

        /// <summary>
        /// 获取全部默认资源
        /// </summary>
        /// <param name="selector">为空则获取全部</param>
        /// <param name="useNewDbContext"></param>
        /// <returns></returns>
        public virtual async Task<List<TResource>> GetAll(Expression<Func<TResource, bool>> selector = null,
            bool useNewDbContext = false) => await BaseService.GetAll(selector, useNewDbContext);

        /// <summary>
        /// 搜索默认资源
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="pageIndex">从1开始</param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public virtual async Task<SearchResponse<TResource>> Search(
            Expression<Func<TResource, bool>> selector, int pageIndex, int pageSize,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false,
            Expression<Func<TResource, object>> include = null) =>
            await BaseService.Search(selector, pageIndex, pageSize, orderBy, asc, include);

        /// <summary>
        /// 删除默认资源
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveAll(Expression<Func<TResource, bool>> selector) =>
            await BaseService.RemoveAll(selector);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveByKey(TKey key)
        {
            return await BaseService.RemoveByKey<TResource>(key);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual async Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> keys)
        {
            return await BaseService.RemoveByKeys<TResource>(keys.Cast<object>());
        }

        /// <summary>
        /// 创建默认资源
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual async Task<SingletonResponse<TResource>> Create(TResource resource) =>
            await BaseService.Add(resource);

        /// <summary>
        /// 创建默认资源
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public virtual async Task<ListResponse<TResource>> Create(List<TResource> resources) =>
            await BaseService.Add(resources);

        public virtual async Task<int> Count(Expression<Func<TResource, bool>> selector) =>
            await BaseService.Count(selector);
    }
}