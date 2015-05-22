using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy
{
    public static class SqlProxy
    {

        private static readonly ConcurrentDictionary<Type, object> _proxies
            = new ConcurrentDictionary<Type, object>();

        private static readonly ConcurrentDictionary<string, IDbConnection> _connections =
            new ConcurrentDictionary<string, IDbConnection>();


        private static IDbConnection GetConnection(string connectionNameOrString) {
            IDbConnection connection = _connections
                .GetOrAdd(connectionNameOrString, _ => {
                    var connectionString =
                        connectionNameOrString.Contains(";") ? connectionNameOrString : ConfigurationManager.ConnectionStrings[connectionNameOrString].ConnectionString;
                    return new SqlConnection(connectionNameOrString);
                });
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            return connection;
        }

        public static T Create<T>() {

            return default(T);
        }
    }
}
