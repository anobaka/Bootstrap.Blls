using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.ResponseBuilders;
using Bootstrap.Business.Components.Services.Infrastructures;
using Bootstrap.Infrastructures.Models;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Services.CommonServices
{
    public abstract class
        ActiveResourceService<TDbContext, TActiveResource, TKey> : ResourceService<TDbContext, TActiveResource, TKey>
        where TDbContext : DbContext where TActiveResource : ActiveResource

    {
        protected ActiveResourceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<BaseResponse> RemoveAll(Expression<Func<TActiveResource, bool>> selector)
        {
            var resources = await GetAll(selector);
            resources.ForEach(a => a.Active = false);
            await DbContext.SaveChangesAsync();
            return BaseResponseBuilder.Ok;
        }

        /// <inheritdoc />
        /// <summary>
        /// 删除，动态缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override async Task<BaseResponse> RemoveByKey(TKey key)
        {
            var resource = await GetByKey(key);
            resource.Active = false;
            await DbContext.SaveChangesAsync();
            return BaseResponseBuilder.Ok;
        }

        /// <inheritdoc />
        /// <summary>
        /// 删除，动态缓存
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override async Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> keys)
        {
            var resources = await GetByKeys(keys);
            resources.ForEach(t => t.Active = false);
            await DbContext.SaveChangesAsync();
            return BaseResponseBuilder.Ok;
        }
    }
}