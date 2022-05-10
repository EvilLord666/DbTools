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
                return new MySqlParameter(parameterName, (MySqlDbType)parameterType)
                {
                    Direction = parameterDirection
                };
            if (dbEngine == DbEngine.PostgresSql)
                return new NpgsqlParameter(parameterName, (NpgsqlDbType) parameterType)
                {
                    Direction = parameterDirection
                };
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

        private static readonly IDictionary<SqlDbType, Type> SqlServerTypesMapping = new Dictionary<SqlDbType, Type>()
        {
            { SqlDbType.BigInt, typeof(long)},
            { SqlDbType.Binary, typeof(byte[])},
            { SqlDbType.Bit, typeof(bool)},
            { SqlDbType.Char, typeof(string)},
            { SqlDbType.DateTime, typeof(DateTime)},
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset)},
            { SqlDbType.Decimal, typeof(decimal)},
            { SqlDbType.Float, typeof(DateTime)}
        };
    }
}