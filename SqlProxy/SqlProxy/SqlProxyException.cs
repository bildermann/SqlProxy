using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {
    public class SqlProxyException : Exception {
        public SqlProxyException() : base() { }
        public SqlProxyException(string message)
            : base(message) {
        }
    }
}
