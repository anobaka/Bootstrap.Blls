using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.Services.Infrastructures;
using Bootstrap.Infrastructures.Components.Extensions;
using Bootstrap.Infrastructures.Models;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Services.CommonServices
{
    public class
        MultilevelResourceService<TDbContext, TMultilevelResource, TKey> : ResourceService<TDbContext, TMultilevelResource,
            TKey>
        where TDbContext : DbContext where TMultilevelResource : MultilevelResource<TMultilevelResource>
    {
        public MultilevelResourceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<TMultilevelResource>> GetPath(TKey id)
        {
            var resource = await GetByKey(id);
            return resource == null ? null : GetPath(DbContext.Set<TMultilevelResource>(), resource);
        }

        /// <summary>
        /// From up to down.
        /// </summary>
        /// <param name="allResources"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        protected List<TMultilevelResource> GetPath(IEnumerable<TMultilevelResource> allResources, TMultilevelResource child)
        {
            if (child == null)
            {
                return null;
            }

            var path = allResources.Where(t => t.Left < child.Left && t.Right > child.Right).ToList();
            path.Add(child);
            return path.OrderBy(t => t.Left).ToList();
        }

        public async Task<List<TMultilevelResource>> GetFullTree()
        {
            var data = await base.GetAll();
            var root = data.FindAll(t => !t.ParentId.HasValue);
            _populateTree(root, data);
            return root;
        }

        private void _populateTree(IReadOnlyCollection<TMultilevelResource> parents, List<TMultilevelResource> allData)
        {
            if (parents != null && allData != null)
            {
                foreach (var p in parents)
                {
                    p.Children = allData.FindAll(t => t.ParentId == p.Id);
                    p.Children.ForEach(t => t.Parent = p);
                    _populateTree(p.Children, allData);
                }
            }
        }

        /// <summary>
        /// 异步刷新分级关系
        /// </summary>
        /// <returns></returns>
        protected async Task BuildTree()
        {
            var tree = await GetAll();
            tree.BuildTree();
            await DbContext.SaveChangesAsync();
        }

        public override async Task<SingletonResponse<TMultilevelResource>> Create(TMultilevelResource resource)
        {
            var rsp = await base.Create(resource);
            await BuildTree();
            return rsp;
        }

        public override async Task<ListResponse<TMultilevelResource>> Create(List<TMultilevelResource> resources)
        {
            var rsp = await base.Create(resources);
            await BuildTree();
            return rsp;
        }

        public override async Task<BaseResponse> RemoveAll(Expression<Func<TMultilevelResource, bool>> selector)
        {
            var rsp = await base.RemoveAll(selector);
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return rsp;
        }

        public override async Task<BaseResponse> RemoveByKey(TKey key)
        {
            var rsp = await base.RemoveByKey(key);
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return rsp;
        }

        public override async Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> keys)
        {
            var rsp = await base.RemoveByKeys(keys);
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return rsp;
        }
    }
}