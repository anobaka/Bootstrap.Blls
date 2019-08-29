using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Business.Components.Services.Infrastructures;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Bootstrap.Business.Components.Extensions
{
    public static class MockExtensions
    {
        public static async Task<T> GetOrCreate<TDbContext, T>(ResourceService<TDbContext, T, object> service,
            Expression<Func<T, bool>> first,
            object createModel) where T : class where TDbContext : DbContext

        {
            var instance = await service.GetFirst(first);
            if (instance == null)
            {
                var serviceType = service.GetType();
                var methods = serviceType.GetMethods();
                var method = methods
                                 .FirstOrDefault(t =>
                                     t.Name.Equals("Add") && t.GetParameters().FirstOrDefault()?.ParameterType.Name
                                         .Contains("CreateRequestModel") == true) ??
                             methods
                                 .FirstOrDefault(t =>
                                     t.Name.Equals("Add") && t.GetParameters().FirstOrDefault()?.ParameterType.Name
                                         .Equals(SpecificTypeUtils<T>.Type.Name) == true);
                instance = (await (method.Invoke(service, new[] {createModel}) as Task<SingletonResponse<T>>)).Data;
            }

            return instance;
        }
    }
}