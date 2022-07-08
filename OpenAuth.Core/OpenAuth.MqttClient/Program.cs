using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.MqttClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string LogFilePath(string LogEvent) => $@"Logs\{LogEvent}\{DateTime.Now:yyyyMMdd}\.log";
            string SerilogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}{Message}{NewLine}" + new string('-', 50) + "{NewLine}";
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                //.WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.File(LogFilePath("Debug"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: SerilogOutputTemplate, fileSizeLimitBytes: 10 * 1048576))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.File(LogFilePath("Error"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: SerilogOutputTemplate, fileSizeLimitBytes: 10 * 1048576))
                .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.File(LogFilePath("Info"), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: SerilogOutputTemplate, fileSizeLimitBytes: 10 * 1048576))
                .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                .UseSerilog();
            });
    }
}
