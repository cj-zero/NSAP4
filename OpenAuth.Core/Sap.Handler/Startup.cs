using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Infrastructure.AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neware.Cap.DependencyInjection;
using OpenAuth.Repository.Extensions;
using Sap.Handler.Autofac;
using Sap.Handler.DependencyInjection;
using Sap.Handler.Service;

namespace Sap.Handler
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
            //≈‰÷√autoMapper
            services.AddAutoMapper();

            services.AddDbContexts();

            services.AddSap(Configuration);

            services.AddNewareCAP(Configuration);

            services.AddSingleton<ServiceOrderSapHandler>();
            services.AddSingleton<SellOrderSapHandler>();
            services.AddSingleton<MaterialSapHandler>();
            services.AddScoped<AfterSaleReturnHandler>();
            services.AddControllersWithViews();
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
            // ≈‰÷√æ≤Ã¨autoMapper
            AutoMapperHelper.UseStateAutoMapper(app);

            app.UseRouting();
            app.UseEndpoints(config => 
            {
                config.MapDefaultControllerRoute();
            });
        }
    }
}
