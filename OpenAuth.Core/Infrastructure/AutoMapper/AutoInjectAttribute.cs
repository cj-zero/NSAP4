using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.AutoMapper
{
    public sealed class AutoInjectAttribute : Attribute
    {
        public Type SourceType { get; }
        public Type TargetType { get; }

        public bool ReverseMap { get; }

        public AutoInjectAttribute(Type sourceType, Type targetType, bool reverseMap = true)
        {
            SourceType = sourceType;
            TargetType = targetType;
            ReverseMap = reverseMap;
        }
    }
}
