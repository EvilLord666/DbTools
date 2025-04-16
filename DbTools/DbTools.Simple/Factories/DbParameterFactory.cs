using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbTools.Core;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;

namespace DbTools.Simple.Factories
{
    public static class DbParameterFactory
    {
        public static DbParameter Create(DbEngine dbEngine, string parameterName, int parameterType, object value,
            ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            if (dbEngine == DbEngine.SqlServer)
            {
                SqlDbType type = (SqlDbType)parameterType;
                return new SqlParameter(parameterName, type)
                {
                    Direction = parameterDirection,
                    Value = MapSqlParameterValue(type, value)
                };
            }

            if (dbEngine == DbEngine.MySql)
            {
                MySqlDbType type = (MySqlDbType)parameterType;
                return new MySqlParameter(parameterName, type)
                {
                    Direction = parameterDirection,
                    Value = MapMySqlParameterValue(type, value)
                };
            }

            if (dbEngine == DbEngine.PostgresSql)
            {
                NpgsqlDbType type = (NpgsqlDbType)parameterType;
                return new NpgsqlParameter(parameterName, type)
                {
                    Direction = parameterDirection,
                    Value = MapPostgresParameterValue(type, value)
                };
            }

            throw new NotImplementedException("Other db engine are not supported yet, please add a github issue https://github.com/EvilLord666/DbTools");
        }

        private static object MapSqlParameterValue(SqlDbType type, object value)
        {
            try
            {
                if (!SqlServerTypesMapping.ContainsKey(type))
                    return value;
                return Convert.ChangeType(value, SqlServerTypesMapping[type]);
            }
            catch (Exception e)
            {
                return value;
            }
        }

        private static object MapMySqlParameterValue(MySqlDbType type, object value)
        {
            try
            {
                if (!MySqlTypesMapping.ContainsKey(type))
                    return value;
                return Convert.ChangeType(value, MySqlTypesMapping[type]);
            }
            catch (Exception e)
            {
                return value;
            }
        }

        private static object MapPostgresParameterValue(NpgsqlDbType type, object value)
        {
            try
            {
                if (!PostgresTypesMapping.ContainsKey(type))
                    return value;
                return Convert.ChangeType(value, PostgresTypesMapping[type]);
            }
            catch (Exception e)
            {
                return value;
            }
        }

        private static readonly IDictionary<SqlDbType, Type> SqlServerTypesMapping = new Dictionary<SqlDbType, Type>()
        {
            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.Binary, typeof(byte[]) },
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Float, typeof(float) },
            { SqlDbType.Image, typeof(byte[]) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.NText, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Real, typeof(Single) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.SmallInt, typeof(Int16) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.Text, typeof(string) },
            { SqlDbType.Timestamp, typeof(byte[]) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.VarBinary, typeof(byte[]) },
            { SqlDbType.VarChar, typeof(string) }
        };

        private static readonly IDictionary<MySqlDbType, Type> MySqlTypesMapping = new Dictionary<MySqlDbType, Type>()
        {
            { MySqlDbType.Decimal, typeof(decimal) },
            { MySqlDbType.Byte, typeof(byte) },
            { MySqlDbType.Int16, typeof(Int16) },
            { MySqlDbType.Int24, typeof(int) },
            { MySqlDbType.Int32, typeof(int) },
            { MySqlDbType.Int64, typeof(long) },
            { MySqlDbType.Float, typeof(Single) },
            { MySqlDbType.Double, typeof(double) },
            { MySqlDbType.Timestamp, typeof(DateTime) },
            { MySqlDbType.Date, typeof(DateTime) },
            { MySqlDbType.Time, typeof(string) },
            { MySqlDbType.DateTime, typeof(DateTime) },
            { MySqlDbType.Datetime, typeof(DateTime) },
            { MySqlDbType.Year, typeof(string) },
            { MySqlDbType.Newdate, typeof(DateTime) },
            { MySqlDbType.Year, typeof(string) },
            { MySqlDbType.VarString, typeof(string) },
            { MySqlDbType.NewDecimal, typeof(decimal) },
            { MySqlDbType.TinyBlob, typeof(byte[]) },
            { MySqlDbType.MediumBlob, typeof(byte[]) },
            { MySqlDbType.LongBlob, typeof(byte[]) },
            { MySqlDbType.Blob, typeof(byte[]) },
            { MySqlDbType.VarChar, typeof(string) },
            { MySqlDbType.String, typeof(string) },
            { MySqlDbType.UByte, typeof(byte) },
            { MySqlDbType.UInt16, typeof(UInt16) },
            { MySqlDbType.UInt24, typeof(UInt32) },
            { MySqlDbType.UInt32, typeof(UInt32) },
            { MySqlDbType.UInt64, typeof(UInt64) },
            { MySqlDbType.Binary, typeof(byte[]) },
            { MySqlDbType.VarBinary, typeof(byte[]) },
            { MySqlDbType.TinyText, typeof(string) },
            { MySqlDbType.MediumText, typeof(string) },
            { MySqlDbType.LongText, typeof(string) },
            { MySqlDbType.Text, typeof(string) }
        };

        private static readonly IDictionary<NpgsqlDbType, Type> PostgresTypesMapping = new Dictionary<NpgsqlDbType, Type>() 
        {
            { NpgsqlDbType.Bigint, typeof(long) },
            { NpgsqlDbType.Bit, typeof(string) },
            { NpgsqlDbType.Boolean, typeof(bool) },
            { NpgsqlDbType.Bytea, typeof(byte[]) },
            { NpgsqlDbType.Char, typeof(string) },
            { NpgsqlDbType.Date, typeof(DateTime) },
            { NpgsqlDbType.Timestamp, typeof(DateTime) },
            { NpgsqlDbType.Double, typeof(double) },
            { NpgsqlDbType.Integer, typeof(int) },
            { NpgsqlDbType.Text, typeof(string) },
            { NpgsqlDbType.Integer, typeof(int) },
            { NpgsqlDbType.Varchar, typeof(string) },
            { NpgsqlDbType.Real, typeof(double) },
            { NpgsqlDbType.Uuid, typeof(string) }
        };
    }
}