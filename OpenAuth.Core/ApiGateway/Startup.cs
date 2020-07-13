using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;

namespace ApiGateway
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            var config = new ConfigurationBuilder().AddJsonFile("ocelot.json", optional: false, reloadOnChange: true).Build();
            services.AddOcelot(config)
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                }).AddPolly();
            services.AddCors();

            //todo: 如果正式 环境请用下面的方式限制随意访问跨域
            //var origins = new[]
            //{
            //    "http://localhost:1803",
            //    "http://localhost:52789"
            //};
            //services.AddCors(option => option.AddPolicy("cors", policy =>
            //      policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(origins)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                app.UseCors("cors");
            }
            //todo:测试可以允许任意跨域，正式环境要加权限
            else
            {
                app.UseCors(builder => builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
            }
           


            app.UseOcelot().Wait();
        }
    }
}
