// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAuth.App;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;

namespace OpenAuth.IdentityServer
{
    public class Startup
    {
        public IHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients(Environment.IsProduction()))
                .AddProfileService<CustomProfileService>();

            services.AddCors();
//          todo:如果正式 环境请用下面的方式限制随意访问跨域
//            var origins = new []
//            {
//                "http://localhost:1803",
//                "http://localhost:52789"
//            };
//            if (Environment.IsProduction())
//            {
//                origins = new []
//                {
//                    "http://demo.openauth.me:1803",
//                    "http://demo.openauth.me:52789"
//                };
//            }
//            services.AddCors(option=>option.AddPolicy("cors", policy =>
//                policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(origins)));

            //全部用测试环境，正式环境请参考https://www.cnblogs.com/guolianyu/p/9872661.html
            //if (Environment.IsDevelopment())
            //{
            builder.AddDeveloperSigningCredential();
            //}
            //else
            //{
            //    throw new Exception("need to configure key material");
            //}

            services.AddAuthentication();
            
            //映射配置文件
            services.Configure<AppSetting>(Configuration.GetSection("AppSetting"));

            //在startup里面只能通过这种方式获取到appsettings里面的值，不能用IOptions😰
            //var dbType = ((ConfigurationSection) Configuration.GetSection("AppSetting:DbType")).Value;
            //if (dbType == Define.DBTYPE_SQLSERVER)
            //{
            //    services.AddDbContext<OpenAuthDBContext>(options =>
            //        options.UseSqlServer(Configuration.GetConnectionString("OpenAuthDBContext")));
            //}
            //else  //mysql
            //{
            //    services.AddDbContext<OpenAuthDBContext>(options =>
            //        options.UseMySql(Configuration.GetConnectionString("OpenAuthDBContext")));
            //}
            services.AddDbContexts();
            var redisConnectionString = Configuration.GetValue<string>("AppSetting:Cache:Redis");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                var csredis = new CSRedis.CSRedisClient(redisConnectionString); RedisHelper.Initialization(csredis);
                services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
            }
        }
        
        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacExt.InitAutofac(builder, Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            //todo:测试可以允许任意跨域，正式环境要加权限
            app.UseCors(builder => builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}