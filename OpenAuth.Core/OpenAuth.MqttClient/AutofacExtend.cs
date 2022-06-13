using Autofac;
using Infrastructure.Cache;
using Infrastructure.Extensions.AutofacManager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
using OpenAuth.App.SSO;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace OpenAuth.MqttClient
{
    public class AutofacExtend
    {
        public static void InitAutofac(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IRepository<>));
            builder.RegisterType(typeof(UnitWork)).As(typeof(IUnitWork));
            builder.RegisterType(typeof(MqttSubscribeApp)).As(typeof(IMqttSubscribe));
            var redisConnectionString = configuration.GetValue<string>("AppSetting:Cache:Redis");
            builder.RegisterType(typeof(LocalAuth)).As(typeof(IAuth));
            //builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                builder.RegisterType(typeof(CacheContext)).As(typeof(ICacheContext));
            else
                builder.RegisterType(typeof(RedisCacheContext)).As(typeof(ICacheContext));

            builder.RegisterType(typeof(HttpContextAccessor)).As(typeof(IHttpContextAccessor));
        }
    }
}
