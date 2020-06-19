using Google.Protobuf.WellKnownTypes;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenAuth.Repository.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDbContexts(this IServiceCollection services)
        {
            InitDbContextDir();
            AddDbContext<OpenAuthDBContext>(services);
            AddDbContext<Nsap4NwcaliDbContext>(services);
            services.AddSingleton<DBContextFactory>();   
            return services;
        }
        private static void AddDbContext<T>(IServiceCollection services) where T : DbContext
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            string connectionString = "";
            var connectionStringName = typeof(T).GetCustomAttributeValue<ConnectionStringAttribute, string>(a => a.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionStringName))
            {
                connectionString = typeof(T).GetCustomAttributeValue<ConnectionStringAttribute, string>(a => a.ConnectionString);
            }
            else
            {
                connectionString = configuration.GetConnectionString(connectionStringName);
            }
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var dbType = ((ConfigurationSection)configuration.GetSection("AppSetting:DbType")).Value;
                if (dbType == "SqlServer")
                {
                    services.AddDbContext<T>(options =>
                        options.UseSqlServer(connectionString));
                }
                else  //mysql
                {
                    services.AddDbContext<T>(options =>
                        options.UseMySql(connectionString));
                }
            }

        }

        private static void InitDbContextDir()
        {
            var dbContexts = AssemblyHelper.GetSubClass(typeof(DbContext));
            foreach (var dbContext in dbContexts)
            {
                foreach (var property in dbContext.GetProperties())
                {
                    if (property.PropertyType.Name.Equals(typeof(DbSet<>).Name))
                    {
                        Common.ContextDir.TryAdd(property.PropertyType.GenericTypeArguments[0], dbContext);
                    }
                }
            }
        }
    }
}
