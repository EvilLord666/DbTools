using System;
using System.Collections.Generic;
using System.Data;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Factories;
using DbTools.Simple.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Xunit;

namespace DbTools.Simple.Tests.Managers
{
    public class TestCommonDbManager
    {
        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "")]
        [InlineData(DbEngine.SqLite, true, null, null)]
        [InlineData(DbEngine.MySql, false, "root", "123")]
        [InlineData(DbEngine.PostgresSql, false, "postgres", "123")]
        public void TestCreateAndDropDb(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password)
        {
            IDbManager dbManager = CreateTestDbManager(dbEngine);
            string connectionString = BuildConnectionString(dbEngine, useIntegratedSecurity, userName, password);
            dbManager.CreateDatabase(connectionString, true);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, true);
            dbManager.DropDatabase(connectionString);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, false);
        }

        [Fact]
        public void TestExecuteNonQuery()
        {
            
        }

        private IDbManager CreateTestDbManager(DbEngine dbEngine)
        {
            return DbManagerFactory.Create(dbEngine, _loggerFactory);
        }

        private string BuildConnectionString(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password)
        {
            Tuple<string, string> hostAndDatabase = _hostAndDatabaseOptions[dbEngine];
            IDictionary<string, string> options = new Dictionary<string, string>();
            options.Add(DbParametersKeys.HostKey, hostAndDatabase.Item1);
            options.Add(DbParametersKeys.DatabaseKey, hostAndDatabase.Item2);
            options.Add(DbParametersKeys.UseIntegratedSecurityKey, useIntegratedSecurity.ToString());
            options.Add(DbParametersKeys.LoginKey, userName);
            options.Add(DbParametersKeys.PasswordKey, password);
            return ConnectionStringBuilder.Build(dbEngine, options);
        }

        private void CheckDatabaseExists(IDbManager dbManager, DbEngine dbEngine, string connectionString, bool expected)
        {
            try
            {
                string cmd = null;
                if (dbEngine == DbEngine.SqlServer)
                    cmd = string.Format(SelectDatabaseTemplate, "name", "master.dbo.sysdatabases",
                        TestSqlServerDatabase);
                if (cmd != null)
                {
                    using (IDataReader reader = dbManager.ExecuteDbReader(connectionString, cmd))
                    {
                        string value = reader.GetString(0);
                        Assert.NotNull(value);
                    }
                }
                else
                {
                    // throw 
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private const string TestSqlServerHost = @"(localdb)\mssqllocaldb";
        private const string TestSqlServerDatabase = "SQLServerTestDb";
        private const string TestSqLiteDatabase = "SqLiteTestDb.sqlite";
        private const string TestMySqlHost = "localhost";
        private const string TestMySqlDatabase = "MySqlTestDb";
        private const string TestPostgresSqlHost = "localhost";
        private const string TestPostgresSqlDatabase = "PostgresTestDb";

        private const string SelectDatabaseTemplate = "SELECT {0} FROM {1} WHERE {0}={2};";

        private readonly ILoggerFactory _loggerFactory = new LoggerFactory();

        private readonly IDictionary<DbEngine, Tuple<string, string>> _hostAndDatabaseOptions =  new Dictionary<DbEngine, Tuple<string, string>>()
        {
            {DbEngine.SqlServer, new Tuple<string, string>(TestSqlServerHost, TestSqlServerDatabase)},
            {DbEngine.SqLite, new Tuple<string, string>(string.Empty, TestSqLiteDatabase)},
            {DbEngine.MySql, new Tuple<string, string>(TestMySqlHost, TestMySqlDatabase)},
            {DbEngine.PostgresSql, new Tuple<string, string>(TestPostgresSqlHost, TestPostgresSqlDatabase)}
        };
    }
}