using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {
    public class SqlProxyParameter {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public object Value { get; set; }
        public SqlParameterType ParameterType { get; set; }

        public SqlProxyParameter() {}

        public SqlProxyParameter(string name, Type dataType, object value, SqlParameterType parameterType) {
            Name = name;
            DataType = dataType;
            Value = value;
            ParameterType = parameterType;
        }
    }

    public enum SqlParameterType {
        None = 0, In = 1, Out = 2
    }
}
