using Autofac;
using Infrastructure.MQTT;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Extensions;
using Serilog;
using System;
using System.Linq;

namespace OpenAuth.MqttClient
{
    public class Startup
    {
        private static MqttNetClient _mqttNetClient;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContexts();
            services.AddControllers();
            services.Configure<AppSetting>(Configuration.GetSection("AppSetting"));
            var mqttConfig = new MqttConfig
            {
                Server = Configuration.GetValue<string>("MqttOption:HostIp"),
                Port = int.Parse(Configuration.GetValue<string>("MqttOption:HostPort")),
                Username = Configuration.GetValue<string>("MqttOption:UserName"),
                Password = Configuration.GetValue<string>("MqttOption:Password"),
                ClientIdentify = Configuration.GetValue<string>("MqttOption:ClientIdentify")
            };
            _mqttNetClient = new MqttNetClient(mqttConfig, MessageReceived, Configuration);
            services.AddSingleton(_mqttNetClient);
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacExtend.InitAutofac(builder, Configuration);
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            ServiceLocator.ApplicationBuilder = app;
            ServiceLocator.serviceProvider = app.ApplicationServices;
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            string topics = e.ApplicationMessage.Topic.Split('/').FirstOrDefault();
            var subscribe = ServiceLocator.serviceProvider?.GetService<IMqttSubscribe>();
            if (topics.Equals("edge_msg"))
            {
                try
                {
                    subscribe?.SubscribeEdgeMsg(e.ApplicationMessage.Payload);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"SubscribeEdgeMsg  {ex.Message}");
                }
            }
            else if (topics.Equals("rt_data"))
            {
                try
                {
                    subscribe?.SubscribeRtData(e.ApplicationMessage.Payload);
                }
                catch(Exception ex)
                {
                    Log.Logger.Error($"SubscribeRtData  {ex.Message}");
                }
            }
        }
    }
}
