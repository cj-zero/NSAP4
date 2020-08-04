﻿// ***********************************************************************
// Assembly         : OpenAuth.Mvc
// Author           : yubaolee
// Created          : 10-26-2015
//
// Last Modified By : yubaolee
// Last Modified On : 10-26-2015
// ***********************************************************************
// <copyright file="AutofacExt.cs" company="www.cnblogs.com/yubaolee">
//     Copyright (c) www.cnblogs.com/yubaolee. All rights reserved.
// </copyright>
// <summary>IOC扩展</summary>
// ***********************************************************************

using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Quartz;
using AutoMapper.Configuration;
using Infrastructure.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using OpenAuth.App.Files;
using OpenAuth.App.Interface;
using OpenAuth.App.SSO;
using OpenAuth.Repository;
using OpenAuth.Repository.Interface;
using IContainer = Autofac.IContainer;

namespace OpenAuth.App
{
    public static  class AutofacExt
    {
        private static IContainer _container;
        public static IContainer InitForTest(IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var builder = new ContainerBuilder();
           
            //注册数据库基础操作和工作单元
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped(typeof(IUnitWork), typeof(UnitWork));

            //注入授权
            builder.RegisterType(typeof(LocalAuth)).As(typeof(IAuth));

            //注册app层
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());

            //防止单元测试时已经注入
            if (services.All(u => u.ServiceType != typeof(ICacheContext)))
            {
                var redisConnectionString = configuration.GetValue<string>("AppSetting:Cache:Redis");
                if(string.IsNullOrWhiteSpace(redisConnectionString))
                    services.AddScoped(typeof(ICacheContext), typeof(CacheContext));
                else
                {
                    var csredis = new CSRedis.CSRedisClient(redisConnectionString); RedisHelper.Initialization(csredis);
                    services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
                }
            }

            if (services.All(u => u.ServiceType != typeof(IHttpContextAccessor)))
            {
                services.AddScoped(typeof(IHttpContextAccessor), typeof(HttpContextAccessor));
            }
            
            builder.RegisterModule(new QuartzAutofacFactoryModule());

            builder.Populate(services);

            _container = builder.Build();
            return _container;

        }
        
        
        public static void InitAutofac(ContainerBuilder builder, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // 注册minio文件服务客户端
            var minioAddress = configuration.GetValue<string>("Minio:Address");
            var minioKey = configuration.GetValue<string>("Minio:AppKey");
            var minioSecret = configuration.GetValue<string>("Minio:AppSecret");
            builder.RegisterInstance<MinioClient>(new MinioClient(minioAddress, minioKey, minioSecret));
            builder.RegisterType<MinioFileStore>().As<IFileStore>();
            //注册数据库基础操作和工作单元
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IRepository<>));
            builder.RegisterType(typeof(UnitWork)).As(typeof(IUnitWork));

            //注入授权
            builder.RegisterType(typeof(LocalAuth)).As(typeof(IAuth));
            //注册app层
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly());

            var redisConnectionString = configuration.GetValue<string>("AppSetting:Cache:Redis");
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                builder.RegisterType(typeof(CacheContext)).As(typeof(ICacheContext));
            else
                builder.RegisterType(typeof(RedisCacheContext)).As(typeof(ICacheContext));

            builder.RegisterType(typeof(HttpContextAccessor)).As(typeof(IHttpContextAccessor));
            
            builder.RegisterModule(new QuartzAutofacFactoryModule());
        }
    }
}