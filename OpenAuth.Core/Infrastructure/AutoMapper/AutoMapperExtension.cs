using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Infrastructure.AutoMapper
{
    public static class AutoMapperExtension
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection service)
        {
            service.TryAddSingleton<MapperConfigurationExpression>();
            service.TryAddSingleton(serviceProvider =>
            {
                var mapperConfigurationExpression = serviceProvider.GetRequiredService<MapperConfigurationExpression>();
                var factory = serviceProvider.GetRequiredService<AutoInjectFactory>();

                foreach (var (sourceType, targetType, reverseMap) in factory.ConvertList)
                {
                    if (reverseMap)
                        mapperConfigurationExpression.CreateMap(sourceType, targetType).ReverseMap();
                    else
                        mapperConfigurationExpression.CreateMap(sourceType, targetType);
                }

                var instance = new MapperConfiguration(mapperConfigurationExpression);

                instance.AssertConfigurationIsValid();

                return instance;
            });
            
            service.TryAddSingleton(serviceProvider =>
            {
                var mapperConfiguration = serviceProvider.GetRequiredService<MapperConfiguration>();

                return mapperConfiguration.CreateMapper();
            });

            return service;
        }

        public static IMapperConfigurationExpression UseAutoMapper(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.ApplicationServices.GetRequiredService<MapperConfigurationExpression>();
        }
        public static void UseAutoInject(this IApplicationBuilder applicationBuilder, params Assembly[] assemblys)
        {
            var factory = applicationBuilder.ApplicationServices.GetRequiredService<AutoInjectFactory>();

            factory.AddAssemblys(assemblys);
        }
    }
}
