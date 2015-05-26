using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
                    return new SqlConnection(connectionString);
                });
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            return connection;
        }

        public static SqlProxyResult ExecuteCommand(string commandText, string connectionNameOrString, Type returnType, SqlProxyCommandType commandType, List<SqlProxyParameter> parameters) {
            var isProc = commandType == SqlProxyCommandType.Procedure;
            var isPrimitive = returnType.IsPrimitiveType();

            var nonExecute = (!isProc && isPrimitive) || (isProc && (returnType == typeof(void) || returnType == typeof(int)));
            
            //Validation
            var valid = true;

            if (!isProc) {
                foreach (var param in parameters) {
                    if (param.ParameterType != SqlParameterType.None) {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid) 
                throw new SqlProxyException(String.Format("{0} cannot use In/Out parameters because it is defined as a function", commandText));

            var conn = GetConnection(connectionNameOrString);

            var command = conn.CreateCommand();
            command.CommandType = isPrimitive || isProc ? CommandType.StoredProcedure : CommandType.Text;

            if (command.CommandType == CommandType.StoredProcedure)
                command.CommandText = commandText;
            else {
                command.CommandText = String.Format("SELECT * FROM {0}({1});", commandText, String.Join(",", parameters.Select(x => String.Concat("@", x.Name))));
            }

            var returnParameter = CreateReturnParameter(command, commandType, returnType);

            if (returnParameter != null)
                command.Parameters.Add(returnParameter);

            foreach (var param in parameters) {
                command.Parameters.Add(CreateParameter(command, param));
            }

            var result = new SqlProxyResult();

            if (nonExecute) {
                var noExecuteResult = command.ExecuteNonQuery();
                if (returnParameter != null) {
                    result.NonExecute = returnParameter.Value;
                } else {
                    result.NonExecute = new EmptyResult();
                }
            } else if (isPrimitive)
                result.Scalar = command.ExecuteScalar();
            else {
                result.Command = command;
            }

            return result;
        }

        static readonly ConcurrentDictionary<Type, bool> _primitiveTypes =
            new ConcurrentDictionary<Type, bool>();

        private static bool IsPrimitiveType(this Type type) {
            return _primitiveTypes.GetOrAdd(type, key => {
                if (key.IsGenericType &&
                    key.GetGenericTypeDefinition() == typeof(Nullable<>))
                    key = key.GetGenericArguments()[0];

                return key == typeof(string) ||
                    key.IsPrimitive || key == typeof(DateTime) ||
                    key == typeof(decimal) || key == typeof(TimeSpan) ||
                    key == typeof(Guid) ||
                    key.IsEnum || key == typeof(byte[]);
            });
        }

        private static IDataParameter CreateReturnParameter(IDbCommand command, SqlProxyCommandType commandType, Type type) {

            if (commandType == SqlProxyCommandType.Procedure) {
                if (!(type == typeof(void) || type == typeof(int)))
                    return null;
            }

            if (!type.IsPrimitiveType())
                return null;

            SqlParameter param = ((SqlCommand)command).CreateParameter();

            param.Direction = ParameterDirection.ReturnValue;
            param.ParameterName = "@RETURN_VALUE";

            param.SqlDbType = !SqlDataMapping.TypeMaps.ContainsValue(type) ? SqlDbType.Structured : SqlDataMapping.SqlTypeMaps[SqlDataMapping.TypeMaps.FirstOrDefault(x => x.Value == type).Key];
            
            return param;
        }


        private static IDataParameter CreateParameter(IDbCommand command, SqlProxyParameter parameter) {
            SqlParameter param = ((SqlCommand) command).CreateParameter();

            if (parameter.ParameterType != SqlParameterType.None) {
                param.Direction = parameter.ParameterType == SqlParameterType.In ? ParameterDirection.Input : ParameterDirection.Output;
            }

            if(!SqlDataMapping.TypeMaps.ContainsValue(parameter.DataType))
                throw new SqlProxyException(String.Format("{0} is not a supported data type", parameter.DataType.FullName));

            param.SqlDbType = SqlDataMapping.SqlTypeMaps[SqlDataMapping.TypeMaps.FirstOrDefault(x => x.Value == parameter.DataType).Key];

            param.ParameterName = String.Concat("@", parameter.Name);
            param.Value = parameter.Value;

            return param;
        }

        public static T ProcessResult<T>(SqlProxyResult result) {
            var type = typeof(T);
            
            IDbCommand command = result.Command;
            object scalar = result.Scalar;
            object nonExecute = result.NonExecute;

            if (type == typeof(EmptyResult)) {
                return (T)((object)command.ExecuteNonQuery());
            }
            else if (command != null) {
                //TODO: Improve reading of values
                var isAsync = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
                type = isAsync ? type.GetGenericArguments()[0] : type;
                var isList = typeof(IList).IsAssignableFrom(type);
                var elementType = isList ? type.GetGenericArguments()[0] : null;

                //TODO: support async
                    
                if (isList) {
                    var props = elementType.GetProperties();
                    using (var reader = command.ExecuteReader()) {
                        var listObj = (IList)Activator.CreateInstance(type);
                        while (reader.Read()) {
                            var obj = Activator.CreateInstance(elementType);
                            foreach (var prop in props) {
                                object value = null;
                                try { value = reader[prop.Name]; } catch { }
                                if (value != null) {
                                    try { value = Convert.ChangeType(value, prop.PropertyType); } catch { }
                                    prop.SetValue(obj, value);
                                }
                            }
                            listObj.Add(obj);
                        }

                        if (isAsync) {
                            var sourceType = typeof(TaskCompletionSource<>).MakeGenericType(type);
                            var source = Activator.CreateInstance(sourceType);
                            sourceType.GetMethod("SetResult").Invoke(source, new object[] { listObj });
                            return (T)sourceType.GetProperty("Task").GetGetMethod().Invoke(source, new object[] { });
                        } else {
                            return (T)listObj;
                        }
                    }
                } else {
                    var props = type.GetProperties();
                    var obj = Activator.CreateInstance(type);
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            foreach (var prop in props) {
                                object value = null;
                                try { value = reader[prop.Name]; } catch { }
                                if (value != null) {
                                    try { value = Convert.ChangeType(value, prop.PropertyType); } catch { }
                                    prop.SetValue(obj, value);
                                }
                            }
                            if (isAsync) {
                                var sourceType = typeof(TaskCompletionSource<>).MakeGenericType(type);
                                var source = Activator.CreateInstance(sourceType);
                                sourceType.GetMethod("SetResult").Invoke(source, new object[] { obj });
                                return (T)sourceType.GetProperty("Task").GetGetMethod().Invoke(source, new object[] { });
                            } else {
                                return (T)obj;
                            }
                        }
                    }
                }
            } else if (nonExecute != null) {
                if (nonExecute is EmptyResult)
                    return default(T);
                try {
                    return (T)Convert.ChangeType(nonExecute, type);
                } catch {
                    return (T)nonExecute;
                }
            } else {
                if (scalar == null)
                    return default(T);
                try {
                    return (T)Convert.ChangeType(scalar, type);
                } catch (Exception) {
                    //Return value as it is if it is not convertiable
                    return (T)scalar;
                }
            }

            return default(T);
        }

        private static ConcurrentDictionary<string, object> _types =
            new ConcurrentDictionary<string, object>();
        
        const TypeAttributes TypeAttribute =
            TypeAttributes.Public | TypeAttributes.Serializable | TypeAttributes.Sealed;

        const BindingFlags MethodBinding = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        const BindingFlags PropertyBinding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        const MethodAttributes MethodAttribute =
            MethodAttributes.Public
            | MethodAttributes.Virtual
            | MethodAttributes.Final
            | MethodAttributes.HideBySig
            | MethodAttributes.NewSlot
            | MethodAttributes.SpecialName;

        private static Type _sqlParameterType = typeof(SqlProxyParameter);
        private static MethodInfo _methodGetTypeHandle = typeof(Type).GetMethod("GetTypeFromHandle", MethodBinding);
        private static ConstructorInfo _parameterCtor = _sqlParameterType.GetConstructors().FirstOrDefault(x => x.GetParameters().Length > 0);
        private static MethodInfo _methodAddParameter = typeof(List<SqlProxyParameter>).GetMethod("Add");
        private static MethodInfo _methodExecuteCommand = typeof(SqlProxy).GetMethod("ExecuteCommand", MethodBinding);
        private static MethodInfo _methodProcessResult = typeof(SqlProxy).GetMethod("ProcessResult", MethodBinding);

        public static T Create<T>() where T : class {
            var cType = typeof(T);
            var fullTypeName = cType.FullName;
            var schemaAttribute = cType.GetCustomAttribute<SchemaContextAttribute>();

            if (!(cType.IsAbstract && cType.IsClass && cType.IsPublic && schemaAttribute != null))
                throw new SqlProxyException(String.Format("{0} must be a public abstract class with attribute of SchemaContextAttribute", fullTypeName));


            var typeName = fullTypeName.Replace(".", string.Empty);
            var asmName = String.Concat(typeName, "Class");

            //Default to typeName if schema name is not defined
            var schema = schemaAttribute.Name ?? cType.Name;
            var connectionStringOrName = schemaAttribute.ConnectionStringOrName;

            if (_types.ContainsKey(asmName))
                return (T)_types[asmName];

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(asmName) {
                    Version = new Version(1, 0, 0, 0)
                },
                AssemblyBuilderAccess.RunAndSave);
            var module = assembly.DefineDynamicModule(String.Concat(assembly.GetName().Name, ".dll"));

            var type = module.DefineType(asmName, TypeAttribute, cType);

            var methods = cType.GetMethods();

            foreach (var method in methods) {

                var funcAttr = method.GetCustomAttribute<SchemaFunctionAttribute>();
                var storeAttr = method.GetCustomAttribute<SchemaStoredProcAttribute>();

                var isValid = method.IsAbstract && method.IsPublic && (funcAttr != null || storeAttr != null);

                var methodName = method.Name;
                var commandType = funcAttr != null ? SqlProxyCommandType.Function : SqlProxyCommandType.Procedure;
                var methodReturnType = method.ReturnType;
                var isVoid = methodReturnType == typeof(void);

                if (!isValid)
                    continue;// throw new SqlProxyException(String.Format("{0} method must be an abstract method for {1}", fullTypeName, methodName));

                var parameters = method.GetParameters();
                var meth = type.DefineMethod(methodName, MethodAttribute, method.ReturnType, parameters.Select(x => x.ParameterType).ToArray());

                var il = meth.GetILGenerator();

                //var resultLocal = il.DeclareLocal(typeof(SqlProxyResult));
                var @params = il.DeclareLocal(typeof(List<SqlProxyParameter>));
                var index = 1;

                //params = new List<SqlProxyParamter>();
                il.Emit(OpCodes.Newobj, @params.LocalType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc, @params);

                foreach (var param in parameters) {
                    var paramParameterType = param.IsOut ? SqlParameterType.Out : param.IsIn ? SqlParameterType.In : SqlParameterType.None;
                    var paramName = param.Name;
                    var paramType = param.ParameterType;
                    var paramLocal = il.DeclareLocal(typeof(SqlProxyParameter));

                    //params = new SqlProxyParameter(paramName, paramType, larg(index), paramParameterType)
                    il.Emit(OpCodes.Ldstr, paramName);

                    il.Emit(OpCodes.Ldtoken, paramType);
                    il.Emit(OpCodes.Call, _methodGetTypeHandle);

                    il.Emit(OpCodes.Ldarg, index++);
                    if (paramType.IsValueType)
                        il.Emit(OpCodes.Box, paramType);

                    il.Emit(OpCodes.Ldc_I4, (int)paramParameterType);
                    

                    il.Emit(OpCodes.Newobj, _parameterCtor);
                    il.Emit(OpCodes.Stloc, paramLocal);

                    //params.Add(param);
                    il.Emit(OpCodes.Ldloc, @params);
                    il.Emit(OpCodes.Ldloc, paramLocal);
                    il.Emit(OpCodes.Callvirt, _methodAddParameter);
                }

                var funcOrProcName = commandType == SqlProxyCommandType.Procedure ? (storeAttr.Name ?? methodName) : (funcAttr.Name ?? methodName);
                var commandText = String.Concat(schema, ".", funcOrProcName);

                //ExecuteCommand(string commandText, string connectionNameOrString, Type returnType, SqlProxyCommandType commandType, List<SqlProxyParameter> parameters)

                il.Emit(OpCodes.Ldstr, commandText);
                
                il.Emit(OpCodes.Ldstr, connectionStringOrName);

                il.Emit(OpCodes.Ldtoken, methodReturnType);
                il.Emit(OpCodes.Call, _methodGetTypeHandle);

                il.Emit(OpCodes.Ldc_I4, (int)commandType);

                il.Emit(OpCodes.Ldloc, @params);

                il.Emit(OpCodes.Call, _methodExecuteCommand);
                //il.Emit(OpCodes.Stloc, resultLocal);

                //il.Emit(OpCodes.Ldloc, resultLocal);
                il.Emit(OpCodes.Call, _methodProcessResult.MakeGenericMethod(isVoid ? typeof(EmptyResult) : methodReturnType));

                if (method.ReturnType == typeof(void))
                    il.Emit(OpCodes.Pop);

                il.Emit(OpCodes.Ret);


                type.DefineMethodOverride(meth, method);
            }


            var createType = type.CreateType();

            return (T)(_types[asmName] = Activator.CreateInstance(createType));
        }

    }
}
