using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Infrastructure.AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neware.Cap.DependencyInjection;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.SignalR;
using OpenAuth.Repository.Extensions;
using OpenAuth.WebApi.Model;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NSAP.App.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //配置autoMapper
            services.AddAutoMapper();

            services.AddDbContexts();

            services.Configure<AppSetting>(Configuration.GetSection("AppSetting"));
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = " AppServer API",
                    Description = "By Neware-R12"
                });

                foreach (var name in Directory.GetFiles(AppContext.BaseDirectory, "*.*",
                    SearchOption.AllDirectories).Where(f => Path.GetExtension(f).ToLower() == ".xml"))
                {
                    option.IncludeXmlComments(name,true);
                }
                option.OperationFilter<GlobalHttpHeaderOperationFilter>(); // 添加httpHeader参数
            });
           
            //SignalR
            services.AddNsapSignalR(Configuration);
            ///CAP
            services.AddNewareCAP(Configuration);
            services.AddControllersWithViews(option =>
            {
                option.Filters.Add<OpenAuthFilter>();
            }).AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                //options.SerializerSettings.ContractResolver = new DefaultContractResolver();    
                options.SerializerSettings.DateFormatString = "yyyy.MM.dd HH:mm:ss";
            });
            var redisConnectionString = Configuration.GetValue<string>("AppSetting:Cache:Redis");
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                services.AddMemoryCache();
            else
            {
                var csredis = new CSRedis.CSRedisClient(redisConnectionString); RedisHelper.Initialization(csredis);
                services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
            }
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacExtension.InitAutofac(builder, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 配置静态autoMapper
            AutoMapperHelper.UseStateAutoMapper(app);
            app.UseRouting();

            app.UseEndpoints(config =>
            {
                config.MapDefaultControllerRoute();
            });
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "NewareAI Server";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                c.DocExpansion(DocExpansion.None);
                c.OAuthClientId("OpenAuth.WebApi");  //oauth客户端名称
                c.OAuthAppName("开源版webapi认证"); // 描述
            });
        }
    }
}
