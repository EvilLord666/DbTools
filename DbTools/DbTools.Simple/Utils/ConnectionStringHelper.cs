using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using DbTools.Core;
using MySql.Data.MySqlClient;
using Npgsql;

namespace DbTools.Simple.Utils
{
    public static class ConnectionStringHelper
    {
        public static string GetDatabaseName(string connectionString, DbEngine dbEngine)
        {
            if (dbEngine == DbEngine.SqlServer)
            {
                SqlConnectionStringBuilder sqlConnStringBuilder = new SqlConnectionStringBuilder(connectionString);
                return sqlConnStringBuilder.InitialCatalog;
            }

            if (dbEngine == DbEngine.SqLite)
            {
                SQLiteConnectionStringBuilder sqLiteConnStringBuilder = new SQLiteConnectionStringBuilder(connectionString);
                return sqLiteConnStringBuilder.DataSource;
            }

            if (dbEngine == DbEngine.MySql)
            {
                MySqlConnectionStringBuilder mySqlConnStringBuilder = new MySqlConnectionStringBuilder(connectionString);
                return mySqlConnStringBuilder.Database;
            }

            if (dbEngine == DbEngine.PostgresSql)
            {
                NpgsqlConnectionStringBuilder postgresConnStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
                return postgresConnStringBuilder.Database;
            }

            throw new NotImplementedException("Other db engine are not supported yet, please add a github issue https://github.com/EvilLord666/DbTools");
        }

        public static string GetSqlServerMasterConnectionString(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = SqlServerMasterDatabase;
            return builder.ConnectionString;
        }

        public static string GetMySqlSysDbConnectionString(string connectionString)
        {
            MySqlConnectionStringBuilder mySqlConnStringBuilder = new MySqlConnectionStringBuilder(connectionString);
            mySqlConnStringBuilder.Database = MySqlDatabase;
            return mySqlConnStringBuilder.ConnectionString;
        }
        
        public static string GetPostgresSqlSysDbConnectionString(string connectionString)
        {
            NpgsqlConnectionStringBuilder pgSqlConnStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            pgSqlConnStringBuilder.Database = PostgresSqlDatabase;
            return pgSqlConnStringBuilder.ConnectionString;
        }

        private const string SqlServerMasterDatabase = "master";
        private const string MySqlDatabase = "mysql";
        private const string PostgresSqlDatabase = "postgres";
    }
}