using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure
{
    public static class AssemblyHelper
    {
        public static IList<Type> GetSubClass(Type baseType)
        {
            var subTypeQuery = from t in Assembly.GetCallingAssembly().GetTypes()
                               where baseType.Equals(t.BaseType)
                               select t;
            return subTypeQuery.ToList();
        }
    }
}
