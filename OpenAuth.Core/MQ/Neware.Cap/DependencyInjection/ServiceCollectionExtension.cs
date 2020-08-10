using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Xsl;

namespace Neware.Cap.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddNewareCAP(this IServiceCollection services, IConfiguration configuration)
        {
            CapSettings capSettings = new CapSettings();
            configuration.GetSection("CapSettings").Bind(capSettings);

            services.AddCap(config => {
                if (capSettings.UseDashboard)
                {
                    config.UseDashboard();
                }
                config.UseMySql(capSettings.MySqlConnectionString);
                config.UseRabbitMQ(options =>
                {
                    options.HostName = capSettings.RabbitMq.HostName;
                    options.Port = capSettings.RabbitMq.Prot;
                    options.UserName = capSettings.RabbitMq.UserName;
                    options.Password = capSettings.RabbitMq.Password;
                });

                config.SucceedMessageExpiredAfter = capSettings.SucceedMessageExpiredAfter;
                config.FailedRetryCount = capSettings.FailedRetryCount;
                config.FailedRetryInterval = capSettings.FailedRetryInterval;
                config.ConsumerThreadCount = capSettings.ConsumerThreadCount;
            });
            return services;
        }
    }
}
