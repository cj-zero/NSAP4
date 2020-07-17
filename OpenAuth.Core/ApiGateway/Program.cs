using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {       
            // ���� Serilog 
            Log.Logger = new LoggerConfiguration()
                // ��С����־�������
                .MinimumLevel.Information()
                // ��־�����������ռ������ Microsoft ��ͷ��������־�����С����Ϊ Information
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                // ������־���������̨
                .WriteTo.Console()
                // ������־������ļ����ļ��������ǰ��Ŀ�� logs Ŀ¼��
                // �ռǵ���������Ϊÿ��
                .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day)
                // ���� logger
                .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    // �� Serilog ����Ϊ��־��¼�ṩ����
                    .UseSerilog(); ;
                });
    }
}
