using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.MQTT
{
    public static class ServiceLocator
    {
        public static IServiceProvider serviceProvider { get; set; }

        public static IApplicationBuilder ApplicationBuilder { get; set; }
        //public static void SetServices(IServiceProvider services)
        //{
        //    Services = services;
        //}
    }
}
