using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using DbTools.Core;
using MySql.Data.MySqlClient;
using Npgsql;

namespace DbTools.Simple.Utils
{
    public static class ConnectionStringBuilder
    {
        public static string Build(DbEngine dbEngine, IDictionary<string, string> parameters)
        {
            if (dbEngine == DbEngine.SqlServer)
                return BuildSqlServerConnectionString(parameters);
            if (dbEngine == DbEngine.SqLite)
                return BuildSqLiteConnectionString(parameters);
            if (dbEngine == DbEngine.MySql)
                return BuildMysqlConnectionString(parameters);
            if (dbEngine == DbEngine.PostgresSql)
                return BuildPostgresSqlConnectionString(parameters);
            throw new NotImplementedException("Other db engine are not supported yet, please add a github issue https://github.com/EvilLord666/ReportGenerator");

        }
        
        private static string BuildSqlServerConnectionString(IDictionary<string, string> parameters)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            if (parameters.ContainsKey(DbParametersKeys.HostKey))
                builder.DataSource = parameters[DbParametersKeys.HostKey];
            if (parameters.ContainsKey(DbParametersKeys.DatabaseKey))
                builder.InitialCatalog = parameters[DbParametersKeys.DatabaseKey];
            if (parameters.ContainsKey(DbParametersKeys.LoginKey) && !parameters.ContainsKey(DbParametersKeys.UseIntegratedSecurityKey))
                builder.UserID = parameters[DbParametersKeys.LoginKey];
            if (parameters.ContainsKey(DbParametersKeys.PasswordKey) && !parameters.ContainsKey(DbParametersKeys.UseIntegratedSecurityKey))
                builder.Password = parameters[DbParametersKeys.PasswordKey];
            if (parameters.ContainsKey(DbParametersKeys.UseIntegratedSecurityKey))
                builder.IntegratedSecurity = Convert.ToBoolean(parameters[DbParametersKeys.UseIntegratedSecurityKey]);
            if (parameters.ContainsKey(DbParametersKeys.UseTrustedConnectionKey))
                builder.TrustServerCertificate = Convert.ToBoolean(parameters[DbParametersKeys.UseTrustedConnectionKey]);
            if (parameters.ContainsKey(DbParametersKeys.ConnectionTimeOutKey))
            {
                Int32 connectionTimeOut = 0;
                bool result = Int32.TryParse(parameters[DbParametersKeys.ConnectionTimeOutKey], out connectionTimeOut);
                if (result)
                    builder.ConnectTimeout = connectionTimeOut;
            }
            return builder.ConnectionString;
        }

        private static string BuildSqLiteConnectionString(IDictionary<string, string> parameters)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            if (parameters.ContainsKey(DbParametersKeys.DatabaseKey))
                builder.DataSource = parameters[DbParametersKeys.DatabaseKey];
            if (parameters.ContainsKey(DbParametersKeys.DatabaseEngineVersionKey))
                builder.Version = Convert.ToInt32(parameters[DbParametersKeys.DatabaseEngineVersionKey]);
            // builder.Pooling = false;
            // builder.JournalMode = SQLiteJournalModeEnum.Off;
            if (parameters.ContainsKey(DbParametersKeys.CommandTimeOutKey))
            {
                Int32 commandTimeOut = 0;
                bool result = Int32.TryParse(parameters[DbParametersKeys.CommandTimeOutKey], out commandTimeOut);
                if (result)
                    builder.DefaultTimeout = commandTimeOut;
            }

            return builder.ConnectionString;
        }

        private static string BuildMysqlConnectionString(IDictionary<string, string> parameters)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            if (parameters.ContainsKey(DbParametersKeys.HostKey))
                builder.Server = parameters[DbParametersKeys.HostKey];
            if (parameters.ContainsKey(DbParametersKeys.DatabaseKey))
                builder.Database = parameters[DbParametersKeys.DatabaseKey].ToLower(); // otherwise is not working
            if (parameters.ContainsKey(DbParametersKeys.LoginKey))
                builder.UserID = parameters[DbParametersKeys.LoginKey];
            if (parameters.ContainsKey(DbParametersKeys.PasswordKey))
                builder.Password = parameters[DbParametersKeys.PasswordKey];
            
            if (parameters.ContainsKey(DbParametersKeys.ConnectionLifeTimeKey))
            {
                UInt32 connectionLifeTime = 0;
                bool result = UInt32.TryParse(parameters[DbParametersKeys.ConnectionLifeTimeKey], out connectionLifeTime);
                if (result)
                    builder.ConnectionLifeTime = connectionLifeTime;
            }
            
            if (parameters.ContainsKey(DbParametersKeys.ConnectionTimeOutKey))
            {
                UInt32 connectionTimeOut = 0;
                bool result = UInt32.TryParse(parameters[DbParametersKeys.ConnectionTimeOutKey], out connectionTimeOut);
                if (result)
                    builder.ConnectionTimeout = connectionTimeOut;
            }
            
            if (parameters.ContainsKey(DbParametersKeys.CommandTimeOutKey))
            {
                UInt32 commandTimeOut = 0;
                bool result = UInt32.TryParse(parameters[DbParametersKeys.CommandTimeOutKey], out commandTimeOut);
                if (result)
                    builder.DefaultCommandTimeout = commandTimeOut;
            }
            
            // todo:umv: handle this too ...
            builder.SslMode = MySqlSslMode.None; 
            if (parameters.ContainsKey(DbParametersKeys.SslModeKey))
            {
                // builder.SslMode = parameters[DbParametersKeys.SslModeKey];
            }

            return builder.ConnectionString;
        }

        private static string BuildPostgresSqlConnectionString(IDictionary<string, string> parameters)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder();
            if (parameters.ContainsKey(DbParametersKeys.HostKey))
                builder.Host = parameters[DbParametersKeys.HostKey];
            if (parameters.ContainsKey(DbParametersKeys.DatabaseKey))
                builder.Database = parameters[DbParametersKeys.DatabaseKey].ToLower(); // otherwise is not working, mysql way;
            if (parameters.ContainsKey(DbParametersKeys.LoginKey))
                builder.Username = parameters[DbParametersKeys.LoginKey];
            if (parameters.ContainsKey(DbParametersKeys.PasswordKey))
                builder.Password = parameters[DbParametersKeys.PasswordKey];
            if (parameters.ContainsKey(DbParametersKeys.ConnectionLifeTimeKey))
            {
                Int32 connectionLifeTime = 0;
                bool result = Int32.TryParse(parameters[DbParametersKeys.ConnectionLifeTimeKey], out connectionLifeTime);
                if (result)
                {
                    builder.ConnectionLifetime = connectionLifeTime;
                    builder.ConnectionIdleLifetime = connectionLifeTime;
                }
            }
            
            if (parameters.ContainsKey(DbParametersKeys.CommandTimeOutKey))
            {
                Int32 commandTimeOut = 0;
                bool result = Int32.TryParse(parameters[DbParametersKeys.CommandTimeOutKey], out commandTimeOut);
                if (result)
                    builder.CommandTimeout = commandTimeOut;
            }
            
            return builder.ConnectionString;
        }
    }
}