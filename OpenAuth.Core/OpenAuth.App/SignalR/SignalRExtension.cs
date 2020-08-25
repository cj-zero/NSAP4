using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.SignalR
{
    public static class SignalRExtension
    {
        public static IServiceCollection AddNsapSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            var redis = configuration.GetSection("AppSetting:SignalR").GetValue<string>("Redis");
            services.AddSignalR().AddMessagePackProtocol().AddRedis(redis);
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            return services;
        }

        public static HubEndpointConventionBuilder MapMessageHub(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapHub<MessageHub>("/MessageHub");//.RequireAuthorization();
        }
    }
}
