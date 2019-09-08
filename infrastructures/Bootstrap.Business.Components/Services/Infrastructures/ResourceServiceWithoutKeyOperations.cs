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
    public class ResourceServiceWithoutKeyOperations<TDbContext, TResource> : ServiceBase<TDbContext>
        where TDbContext : DbContext where TResource : class
    {
        protected BaseService<TDbContext> BaseService;

        public ResourceServiceWithoutKeyOperations(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            BaseService = serviceProvider.GetRequiredService<BaseService<TDbContext>>();
        }

        /// <summary>
        /// 获取单条默认资源
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="orderBy"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public virtual Task<TResource> GetFirst(Expression<Func<TResource, bool>> selector,
            Expression<Func<TResource, object>> orderBy = null,
            bool asc = false) => BaseService.GetFirst(selector, orderBy, asc);

        /// <summary>
        /// 获取全部默认资源
        /// </summary>
        /// <param name="selector">为空则获取全部</param>
        /// <param name="useNewDbContext"></param>
        /// <returns></returns>
        public virtual Task<List<TResource>> GetAll(Expression<Func<TResource, bool>> selector = null,
            bool useNewDbContext = false) => BaseService.GetAll(selector, useNewDbContext);

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
        public virtual Task<SearchResponse<TResource>> Search(
            Expression<Func<TResource, bool>> selector, int pageIndex, int pageSize,
            Expression<Func<TResource, object>> orderBy = null, bool asc = false,
            Expression<Func<TResource, object>> include = null) =>
            BaseService.Search(selector, pageIndex, pageSize, orderBy, asc, include);

        public virtual Task<BaseResponse> Remove(TResource resource) => BaseService.Remove(resource);
        public virtual Task<BaseResponse> RemoveRange(IEnumerable<TResource> resources) => BaseService.RemoveRange(resources);

        /// <summary>
        /// 删除默认资源
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual Task<BaseResponse> RemoveAll(Expression<Func<TResource, bool>> selector) =>
            BaseService.RemoveAll(selector);

        /// <summary>
        /// 创建默认资源
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual Task<SingletonResponse<TResource>> Add(TResource resource) => BaseService.Add(resource);

        /// <summary>
        /// 创建默认资源
        /// </summary>
        /// <param name="resources"></param>
        /// <returns></returns>
        public virtual Task<ListResponse<TResource>> AddRange(List<TResource> resources) =>
            BaseService.AddRange(resources);

        public virtual Task<int> Count(Expression<Func<TResource, bool>> selector) => BaseService.Count(selector);

        public virtual Task<BaseResponse> Update(TResource resource) => BaseService.Update(resource);
        public virtual Task<BaseResponse> UpdateRange(IEnumerable<TResource> resources) => BaseService.UpdateRange(resources);

        public virtual Task<SingletonResponse<TResource>> UpdateFirst(Expression<Func<TResource, bool>> selector,
            Action<TResource> modify) => BaseService.UpdateFirst(selector, modify);

        public virtual Task<ListResponse<TResource>> UpdateAll(Expression<Func<TResource, bool>> selector,
            Action<TResource> modify) => BaseService.UpdateAll(selector, modify);
    }
}
