using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;
using Microsoft.AspNetCore.Hosting;
using App.Metrics.AspNetCore;
using System.Linq;
using App.Metrics.Formatters.Prometheus;

namespace Infrastructure
{
    public static class AppMetricsExtension
    {
        public static IHostBuilder ConfigurationPrometheusAppMetrics(this IHostBuilder hostBuilder)
        {

            var Metrics = AppMetrics.CreateDefaultBuilder()
                   .OutputMetrics.AsPrometheusPlainText()
                   .OutputMetrics.AsPrometheusProtobuf()
                   .Build();
            return hostBuilder.ConfigureMetrics(Metrics)
                .UseMetrics(
                    options =>
                    {
                        options.EndpointOptions = endpointsOptions =>
                        {
                            endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.First(f => f.GetType() == typeof(MetricsPrometheusTextOutputFormatter));//.GetType<MetricsPrometheusTextOutputFormatter>();
                            endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.First(f => f.GetType() == typeof(MetricsPrometheusProtobufOutputFormatter));//.GetType<MetricsPrometheusProtobufOutputFormatter>();
                        };
                    });
        }
    }
}
