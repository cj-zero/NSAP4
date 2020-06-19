using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenAuth.Repository
{
    public class DBContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DBContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContext GetDbContext<T>() where T : class
        {
            if (Common.ContextDir.TryGetValue(typeof(T), out Type contextType))
            {
                return (DbContext)_serviceProvider.CreateScope().ServiceProvider.GetRequiredService(contextType);
            }
            var dbContexts = AssemblyHelper.GetSubClass(typeof(DbContext));
            foreach (var dbContext in dbContexts)
            {
                foreach (var property in dbContext.GetProperties())
                {
                    if (property.PropertyType.Equals(typeof(DbSet<T>)))
                    {
                        var dbct = (DbContext)_serviceProvider.CreateScope().ServiceProvider.GetRequiredService(dbContext);
                        Common.ContextDir.TryAdd(typeof(T), dbContext);
                        return dbct;
                    }
                }
            }
            return null;
        }
    }
}
