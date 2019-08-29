using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Bootstrap.Business.Components.Services.Options;
using Bootstrap.Infrastructures.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bootstrap.Business.Components.Services.Infrastructures
{
    public class ServiceBase<TDbContext> where TDbContext : DbContext
    {
        protected IServiceProvider ServiceProvider;
        protected IOptions<ServiceOptions> Options;
        protected ILogger Logger;

        protected ServiceBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            Options = serviceProvider.GetRequiredService<IOptions<ServiceOptions>>();
        }

        #region Services

        protected virtual TDbContext DbContext => GetRequiredService<TDbContext>();
        protected virtual TDbContext NewScopeDbContext => GetRequiredService<TDbContext>(true);

        protected virtual IDbContextTransaction BeginTransaction(bool createNew, out bool isNewTransaction)
        {
            isNewTransaction = createNew || DbContext.Database.CurrentTransaction == null;
            return isNewTransaction ? DbContext.Database.BeginTransaction() : DbContext.Database.CurrentTransaction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Resolved service from current request scope. Or from global scope if it doesn't exist.</returns>
        protected virtual T GetRequiredService<T>(bool fromNewScope = false)
        {
            if (fromNewScope)
            {
                return ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<T>();
            }

            return (ServiceProvider.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.RequestServices ??
                    ServiceProvider).GetRequiredService<T>();
        }

        #endregion
    }
}