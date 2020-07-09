using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.AutoMapper
{
    public static class AtuoMapperConfig
    {
        public static MapperConfigurationExpression ConfigurationExpression(this MapperConfigurationExpression expression)
        {

            return expression;
        }
    }
}
