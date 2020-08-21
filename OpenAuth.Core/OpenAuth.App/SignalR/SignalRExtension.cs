using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.SignalR
{
    public static class SignalRExtension
    {
        public static IServiceCollection AddNsapSignalR(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            return services;
        }

        public static HubEndpointConventionBuilder MapMessageHub(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapHub<MessageHub>("/MessageHub");//.RequireAuthorization();
        }
    }
}
