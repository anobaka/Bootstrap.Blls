using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bootstrap.Business.Components.ResponseBuilders;
using Bootstrap.Business.Components.Services.Infrastructures;
using Bootstrap.Infrastructures.Components.Extensions;
using Bootstrap.Infrastructures.Models;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Services.CommonServices
{
    public class
        ActiveMultilevelResourceService<TDbContext, TActiveMultilevelResource, TKey> : ResourceService<TDbContext,
            TActiveMultilevelResource, TKey>
        where TDbContext : DbContext where TActiveMultilevelResource : ActiveMultilevelResource<TActiveMultilevelResource>
    {
        public ActiveMultilevelResourceService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<TActiveMultilevelResource>> GetPath(TKey id)
        {
            var resource = await GetByKey(id);
            if (resource == null)
            {
                return null;
            }

            return GetPath(DbContext.Set<TActiveMultilevelResource>(), resource);
        }

        public List<TActiveMultilevelResource> GetPath(IEnumerable<TActiveMultilevelResource> allResources, TActiveMultilevelResource child)
        {
            if (child == null)
            {
                return null;
            }

            var path = allResources.Where(t => t.Left < child.Left && t.Right > child.Right).ToList();
            path.Add(child);
            return path.OrderBy(t => t.Left).ToList();
        }

        public async Task<List<TActiveMultilevelResource>> GetFullTree()
        {
            var data = await GetAll(t => t.Active);
            var root = data.FindAll(t => !t.ParentId.HasValue);
            _populateTree(root, data);
            return root;
        }

        private void _populateTree(IReadOnlyCollection<TActiveMultilevelResource> parents, List<TActiveMultilevelResource> allData)
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
        public async Task BuildTree()
        {
            var tree = await GetFullTree();
            tree.BuildTree();
            await DbContext.SaveChangesAsync();
        }

        public override async Task<BaseResponse> RemoveAll(Expression<Func<TActiveMultilevelResource, bool>> selector)
        {
            var resources = await GetAll(selector);
            resources.ForEach(a => a.Active = false);
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return BaseResponseBuilder.Ok;
        }

        public override async Task<SingletonResponse<TActiveMultilevelResource>> Add(TActiveMultilevelResource resource)
        {
            var rsp = await base.Add(resource);
            await BuildTree();
            return rsp;
        }


        public override async Task<BaseResponse> RemoveByKey(TKey key)
        {
            var resource = await GetByKey(key);
            resource.Active = false;
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return BaseResponseBuilder.Ok;
        }

        public override async Task<BaseResponse> RemoveByKeys(IEnumerable<TKey> key)
        {
            var resources = await GetByKeys(key);
            resources.ForEach(a => a.Active = false);
            await DbContext.SaveChangesAsync();
            await BuildTree();
            return BaseResponseBuilder.Ok;
        }

        public override async Task<ListResponse<TActiveMultilevelResource>> AddRange(List<TActiveMultilevelResource> resources)
        {
            var rsp = await base.AddRange(resources);
            await BuildTree();
            return rsp;
        }
    }
}