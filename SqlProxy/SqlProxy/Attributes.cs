using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SchemaContextAttribute : Attribute {
        internal string ConnectionStringOrName { get; set; }
        public string Name { get; set; }
        public SchemaContextAttribute(string connectionStringOrName) {
            ConnectionStringOrName = connectionStringOrName;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SchemaStoredProcAttribute : Attribute {
        internal string Name { get; set; }
        public SchemaStoredProcAttribute(string name) {
            Name = name;
        }
        public SchemaStoredProcAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SchemaFunctionAttribute : Attribute {
        internal string Name { get; set; }
        public SchemaFunctionAttribute(string name) {
            Name = name;
        }
        public SchemaFunctionAttribute() { }
    }
}
