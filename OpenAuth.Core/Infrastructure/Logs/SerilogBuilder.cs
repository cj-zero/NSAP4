using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Infrastructure
{
    public static class SerilogBuilder
    {
        public static LoggerConfiguration BuildSerilogLogger(this LoggerConfiguration loggerConfiguration,IConfiguration configuration)
        {
            var esUrl = configuration["Serilog:ElasticConfiguration:Uri"];
            var esProject = configuration["Serilog:ElasticConfiguration:Project"];
            var userName = configuration["Serilog:ElasticConfiguration:UserName"];
            var password = configuration["Serilog:ElasticConfiguration:Password"];
            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // 配置日志输出到文件，文件输出到当前项目的 logs 目录下
                // 日记的生成周期为每天
                .WriteTo.File(Path.Combine("log", @"log.txt"), rollingInterval: RollingInterval.Day)
                ;
            if (!string.IsNullOrWhiteSpace(esUrl))
                loggerConfiguration = loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(esUrl))
                {
                    MinimumLogEventLevel = LogEventLevel.Warning,
                    AutoRegisterTemplate = true,
                    IndexFormat = $"{esProject}-{DateTime.Now:yyyy.MM.dd}",
                    ModifyConnectionSettings = x => x.BasicAuthentication(userName, password)
                });
            return loggerConfiguration;
        }
    }
}
