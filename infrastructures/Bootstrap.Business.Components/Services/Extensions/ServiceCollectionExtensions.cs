using System;
using System.Collections.Generic;
using System.Text;
using Bootstrap.Business.Components.Services.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bootstrap.Business.Components.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBootstrapServices<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
        {
            services.TryAddSingleton<BaseService<TDbContext>>();
            return services;
        }
    }
}
