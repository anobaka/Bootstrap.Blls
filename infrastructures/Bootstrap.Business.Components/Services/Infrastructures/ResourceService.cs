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
    public class ResourceService<TDbContext, TResource, TKey> : ResourceServiceWithoutKeyOperations<TDbContext, TResource>
        where TDbContext : DbContext where TResource : class
    {
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

        public virtual Task<SingletonResponse<TResource>> UpdateByKey(TKey key, Action<TResource> modify) =>
            BaseService.UpdateByKey(key, modify);

        public virtual Task<ListResponse<TResource>> UpdateByKeys(IReadOnlyCollection<TKey> keys,
            Action<TResource> modify) => BaseService.UpdateByKeys(keys.Cast<object>().ToList(), modify);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual Task<BaseResponse> RemoveByKey(TKey key) => BaseService.RemoveByKey<TResource>(key);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> keys) =>
            BaseService.RemoveByKeys<TResource>(keys.Cast<object>());
    }
}