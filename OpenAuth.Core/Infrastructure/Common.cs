using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class Common
    {
        public static readonly IDictionary<Type, Type> ContextDir = new Dictionary<Type, Type>();
    }
}
