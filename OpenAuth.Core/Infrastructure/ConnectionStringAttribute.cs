using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ConnectionStringAttribute : Attribute
    {
        public ConnectionStringAttribute(string ConnectionStringName)
        {
            this.ConnectionStringName = ConnectionStringName;
        }

        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }

        public string DbType { get; set; }
    }
}
