using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SchemaContextAttribute : Attribute {
        internal string ConnectionStringOrName { get; set; }
        public string Name { get; set; }
        public SchemaContextAttribute(string connectionStringOrName) {
            ConnectionStringOrName = connectionStringOrName;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SchemaMethodAttribute : Attribute {
        internal string Name { get; set; }
        public SchemaMethodAttribute(string name) {
            Name = name;
        }
    }
}
