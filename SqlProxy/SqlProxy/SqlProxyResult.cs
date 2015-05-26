using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {
    public class SqlProxyResult {
        public IDbCommand Command { get; set; }
        public object NonExecute { get; set; }
        public object Scalar { get; set; }
    }
}
