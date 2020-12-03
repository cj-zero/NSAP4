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
            AddDbContext<NsapBoneDbContext>(services);
            AddDbContext<OpenAuthDBContext>(services);
            AddDbContext<Nsap4NwcaliDbContext>(services);
            AddDbContext<NsapBaseDbContext>(services);
            AddDbContext<Nsap4ServeDbContext>(services);
            AddDbContext<Nsap4MaterialDbContext>(services);
            AddDbContext<SapDbContext>(services);

            services.AddSingleton<DBContextFactory>();   
            return services;
        }
        private static void AddDbContext<T>(IServiceCollection services) where T : DbContext
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            string connectionString = "";
            var connectionStringName = typeof(T).GetCustomAttributeValue<ConnectionStringAttribute, string>(a => a.ConnectionStringName);
            var contextDbType = typeof(T).GetCustomAttributeValue<ConnectionStringAttribute, string>(a => a.DbType);
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
                string dbType;
                if (!string.IsNullOrWhiteSpace(contextDbType))
                {
                    dbType = contextDbType;
                }
                else
                    dbType = ((ConfigurationSection)configuration.GetSection("AppSetting:DbType")).Value;

                if (dbType.ToLower() == "sqlserver") //sqlserver
                {
                    services.AddDbContext<T>(options =>
                        options.UseSqlServer(connectionString),ServiceLifetime.Transient);
                }
                else if (dbType.ToLower() == "mysql") //mysql
                {
                    services.AddDbContext<T>(options =>
                        options.UseMySql(connectionString), ServiceLifetime.Transient);
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
                    // || property.PropertyType.Name.Equal(typeof(DbQuery<>).Name)
                    if (property.PropertyType.Name.Equals(typeof(DbSet<>).Name))
                    {
                        Common.ContextDir.TryAdd(property.PropertyType.GenericTypeArguments[0], dbContext);
                    }
                }
            }
        }
    }
}
