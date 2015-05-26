using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlProxy {

    public enum SqlMappingDataType {
        bigint,
        binary,
        bit,
        @char,
        date,
        datetime,
        datetime2,
        datetimeoffset,
        @decimal,
        varbinary,
        @float,
        image,
        @int,
        money,
        nchar,
        ntext,
        numeric,
        nvarchar,
        real,
        rowversion,
        smalldatetime,
        smallint,
        smallmoney,
        sql_variant,
        text,
        time,
        timestamp,
        tinyint,
        uniqueidentifier,
        varchar,
        xml
    }

    internal class SqlDataMapping {
        internal readonly static Dictionary<SqlMappingDataType, SqlDbType> SqlTypeMaps =
            new Dictionary<SqlMappingDataType, SqlDbType> {
                {SqlMappingDataType.bigint, SqlDbType.BigInt},
                {SqlMappingDataType.binary, SqlDbType.Binary},
                {SqlMappingDataType.bit, SqlDbType.Bit},
                {SqlMappingDataType.@char, SqlDbType.Char},
                {SqlMappingDataType.date, SqlDbType.Date},
                {SqlMappingDataType.datetime, SqlDbType.DateTime},
                {SqlMappingDataType.datetime2, SqlDbType.DateTime2},
                {SqlMappingDataType.datetimeoffset, SqlDbType.DateTimeOffset},
                {SqlMappingDataType.@decimal, SqlDbType.Decimal},
                {SqlMappingDataType.varbinary, SqlDbType.VarBinary},
                {SqlMappingDataType.@float, SqlDbType.Float},
                {SqlMappingDataType.image, SqlDbType.Binary},
                {SqlMappingDataType.@int, SqlDbType.Int},
                {SqlMappingDataType.money, SqlDbType.Money},
                {SqlMappingDataType.nchar, SqlDbType.NChar},
                {SqlMappingDataType.ntext, SqlDbType.NText},
                {SqlMappingDataType.numeric, SqlDbType.Decimal},
                {SqlMappingDataType.nvarchar, SqlDbType.NVarChar},
                {SqlMappingDataType.real, SqlDbType.Real},
                {SqlMappingDataType.rowversion, SqlDbType.Timestamp},
                {SqlMappingDataType.smalldatetime, SqlDbType.DateTime},
                {SqlMappingDataType.smallint, SqlDbType.SmallInt},
                {SqlMappingDataType.smallmoney, SqlDbType.SmallMoney},
                {SqlMappingDataType.sql_variant, SqlDbType.Variant},
                {SqlMappingDataType.text, SqlDbType.Text},
                {SqlMappingDataType.time, SqlDbType.Time},
                {SqlMappingDataType.timestamp, SqlDbType.Timestamp},
                {SqlMappingDataType.tinyint, SqlDbType.TinyInt},
                {SqlMappingDataType.uniqueidentifier, SqlDbType.UniqueIdentifier},
                {SqlMappingDataType.varchar, SqlDbType.Variant},
                {SqlMappingDataType.xml, SqlDbType.Xml}
            };

        internal readonly static Dictionary<SqlMappingDataType, Type> TypeMaps =
            new Dictionary<SqlMappingDataType, Type> {
                {SqlMappingDataType.bigint, typeof(long)},
                {SqlMappingDataType.binary, typeof(byte[])},
                {SqlMappingDataType.bit, typeof(bool)},
                {SqlMappingDataType.@char, typeof(string)},
                {SqlMappingDataType.date, typeof(DateTime)},
                {SqlMappingDataType.datetime, typeof(DateTime)},
                {SqlMappingDataType.datetime2, typeof(DateTime)},
                {SqlMappingDataType.datetimeoffset, typeof(DateTimeOffset)},
                {SqlMappingDataType.@decimal, typeof(decimal)},
                {SqlMappingDataType.varbinary, typeof(byte[])},
                {SqlMappingDataType.@float, typeof(double)},
                {SqlMappingDataType.image, typeof(byte[])},
                {SqlMappingDataType.@int, typeof(int)},
                {SqlMappingDataType.money, typeof(decimal)},
                {SqlMappingDataType.nchar, typeof(string)},
                {SqlMappingDataType.ntext, typeof(string)},
                {SqlMappingDataType.numeric, typeof(decimal)},
                {SqlMappingDataType.nvarchar, typeof(string)},
                {SqlMappingDataType.real, typeof(Single)},
                {SqlMappingDataType.rowversion, typeof(byte[])},
                {SqlMappingDataType.smalldatetime, typeof(DateTime)},
                {SqlMappingDataType.smallint, typeof(Int16)},
                {SqlMappingDataType.smallmoney, typeof(decimal)},
                {SqlMappingDataType.sql_variant, typeof(object)},
                {SqlMappingDataType.text, typeof(string)},
                {SqlMappingDataType.time, typeof(TimeSpan)},
                {SqlMappingDataType.timestamp, typeof(byte[])},
                {SqlMappingDataType.tinyint, typeof(byte)},
                {SqlMappingDataType.uniqueidentifier, typeof(Guid)},
                {SqlMappingDataType.varchar, typeof(string)},
                {SqlMappingDataType.xml, typeof(string)}
            };
    }
}
