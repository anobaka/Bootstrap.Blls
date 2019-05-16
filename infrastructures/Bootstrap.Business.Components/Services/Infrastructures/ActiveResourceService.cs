using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Extensions.ResponseBuilders;
using Bootstrap.Infrastructures.Models;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Services.Infrastructures
{
    public abstract class
        ActiveResourceService<TDbContext, TDefaultResource> : AbstractService<TDbContext, TDefaultResource>
        where TDbContext : DbContext where TDefaultResource : ActiveResource

    {
        protected ActiveResourceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<BaseResponse> Delete(Expression<Func<TDefaultResource, bool>> selector)
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
        public override async Task<BaseResponse> DeleteByKey(object key)
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
        public override async Task<BaseResponse> DeleteByKeys(IEnumerable<object> keys)
        {
            var resources = await GetByKeys(keys);
            resources.ForEach(t => t.Active = false);
            await DbContext.SaveChangesAsync();
            return BaseResponseBuilder.Ok;
        }
    }
}