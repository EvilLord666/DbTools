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
                    Direction = parameterDirection
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

        private static readonly IDictionary<SqlDbType, Type> SqlServerTypesMapping = new Dictionary<SqlDbType, Type>()
        {
            { SqlDbType.BigInt, typeof(long)},
            { SqlDbType.Binary, typeof(byte[])},
            { SqlDbType.Bit, typeof(bool)},
            { SqlDbType.Char, typeof(string)},
            { SqlDbType.DateTime, typeof(DateTime)},
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset)},
            { SqlDbType.Decimal, typeof(decimal)},
            { SqlDbType.Float, typeof(float)},
            { SqlDbType.Image, typeof(byte[])},
            { SqlDbType.Int, typeof(int)},
            { SqlDbType.Money, typeof(decimal)},
            { SqlDbType.NChar, typeof(string)},
            { SqlDbType.NText, typeof(string)},
            { SqlDbType.NVarChar, typeof(string)},
            { SqlDbType.Real, typeof(Single)},
            { SqlDbType.SmallDateTime, typeof(DateTime)},
            { SqlDbType.SmallInt, typeof(Int16)},
            { SqlDbType.SmallMoney, typeof(decimal)},
            { SqlDbType.Text, typeof(string)},
            { SqlDbType.Timestamp, typeof(byte[])},
            { SqlDbType.UniqueIdentifier, typeof(Guid)},
            { SqlDbType.VarBinary, typeof(byte[])},
            { SqlDbType.VarChar, typeof(string)}
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
            { MySqlDbType.Datetime, typeof(DateTime) }
        };
    };
    }
}