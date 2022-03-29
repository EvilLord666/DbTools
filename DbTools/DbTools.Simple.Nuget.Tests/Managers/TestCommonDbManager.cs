using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Factories;
using DbTools.Simple.Utils;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DbTools.Simple.Nuget.Tests.Managers
{
    public class TestCommonDbManager
    {
        [Theory]
        [InlineData(DbEngine.MySql, false, "root", "123", true)]
        [InlineData(DbEngine.MySql, false, "root", "123", false)]
        private void TestCreateLongDataWithNupkgLib(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password, bool isAsync)
        {
            IDbManager dbManager = CreateTestDbManager(dbEngine);
            string connectionString = BuildConnectionString(dbEngine, useIntegratedSecurity, userName, password,
                                                            10000, 1200, 1000);
            dbManager.CreateDatabase(connectionString, true);
            string createTablesCmd = File.ReadAllText(Path.GetFullPath(CreateStructureForLongDataTestScriptFile));
            string insertDataCmd = File.ReadAllText(Path.GetFullPath(InsertDataForLongDataTestScriptFile));
            ExecuteScriptAndCheck(dbManager, connectionString, createTablesCmd, isAsync);
            ExecuteScriptAndCheck(dbManager, connectionString, insertDataCmd, isAsync);
        }

        private void ExecuteScriptAndCheck(IDbManager dbManager, string connectionString, string cmd, bool isAsync)
        {
            bool result = false;
            if (isAsync)
            {
                Task<bool> executionTask = dbManager.ExecuteNonQueryAsync(connectionString, cmd);
                executionTask.Wait();
                result = executionTask.Result;
            }
            else result = dbManager.ExecuteNonQuery(connectionString, cmd);
            Assert.True(result);
        }
        
         private IDbManager CreateTestDbManager(DbEngine dbEngine)
        {
            return DbManagerFactory.Create(dbEngine, _loggerFactory);
        }

        private string BuildConnectionString(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password,
                                             int? connectionLifeTime = null, int? connectionTimeout = null, int? commandTimeOut = null)
        {
            Tuple<string, string> hostAndDatabase = _hostAndDatabaseOptions[dbEngine];
            IDictionary<string, string> options = new Dictionary<string, string>();
            options.Add(DbParametersKeys.HostKey, hostAndDatabase.Item1);
            options.Add(DbParametersKeys.DatabaseKey, hostAndDatabase.Item2);
            options.Add(DbParametersKeys.UseIntegratedSecurityKey, useIntegratedSecurity.ToString());
            options.Add(DbParametersKeys.LoginKey, userName);
            options.Add(DbParametersKeys.PasswordKey, password);
            if (connectionLifeTime.HasValue)
                options.Add(DbParametersKeys.ConnectionLifeTimeKey, connectionLifeTime.Value.ToString());
            if (connectionTimeout.HasValue)
                options.Add(DbParametersKeys.ConnectionTimeOutKey, connectionTimeout.Value.ToString());
            if (commandTimeOut.HasValue)
                options.Add(DbParametersKeys.CommandTimeOutKey, commandTimeOut.Value.ToString());
            return ConnectionStringBuilder.Build(dbEngine, options);
        }

        /*private void CheckDatabaseExists(IDbManager dbManager, DbEngine dbEngine, string connectionString, bool expected)
        {
            string cmd = null;
            string sysConnectionString = null;
            string dbName = _hostAndDatabaseOptions[dbEngine].Item2;
            if (dbEngine == DbEngine.SqlServer)
            {
                sysConnectionString = ConnectionStringHelper.GetSqlServerMasterConnectionString(connectionString);
                cmd = string.Format(SelectDatabaseTemplate, "name", "master.dbo.sysdatabases", $"N'{dbName}'");
            }

            if (dbEngine == DbEngine.MySql)
            {
                sysConnectionString = ConnectionStringHelper.GetMySqlSysDbConnectionString(connectionString);
                cmd = string.Format(SelectDatabaseTemplate, "SCHEMA_NAME", "INFORMATION_SCHEMA.SCHEMATA", $"'{dbName.ToLower()}'");
            }
            
            if (dbEngine == DbEngine.PostgresSql)
            {
                sysConnectionString = ConnectionStringHelper.GetPostgresSqlSysDbConnectionString(connectionString);
                cmd = string.Format(SelectDatabaseTemplate, "datname", "pg_database", $"'{dbName.ToLower()}'");
            }

            if (dbEngine == DbEngine.SqLite)
            {
                Assert.Equal(expected, File.Exists(dbName));
                return;
            }

            if (cmd != null)
            {
                string result = string.Empty;
                Tuple<IDataReader, IDbConnection> reader = dbManager.ExecuteDbReader(sysConnectionString, cmd);
                while (reader.Item1.Read())
                    result = reader.Item1.GetString(0);
                reader.Item1.Close();
                reader.Item1.Dispose();
                reader.Item2.Close();
                if (expected)
                    Assert.Equal(dbName.ToLower(), result.ToLower());
                else Assert.Equal(string.Empty, result);
            }
            else
            {
                throw new InvalidOperationException("Unable to check Db existence");
            }
        }*/

        private const string TestMySqlHost = "localhost";
        private const string TestSqlServerHost = @"(localdb)\mssqllocaldb";
        private const string TestPostgresSqlHost = "localhost";
        
        private const string TestSqlServerDatabase = "SQLServerTestDb_{0}";
        private const string TestSqLiteDatabase = "SqLiteTestDb_{0}.sqlite";     
        private const string TestMySqlDatabase = "MySqlTestDb_{0}";    
        private const string TestPostgresSqlDatabase = "PostgresTestDb_{0}";

        //private const string SelectDatabaseTemplate = "SELECT {0} FROM {1} WHERE {0}={2};";
        //private const string SelectCitiesQuery = "SELECT Id, Name, RegionId FROM City";
        //private const string CreateStructureScriptFile = @"..\..\..\TestScripts\CreateDb.sql";
        //private const string InsertDataScriptFile = @"..\..\..\TestScripts\InsertData.sql";
        private const string CreateStructureForLongDataTestScriptFile = @"..\..\..\TestScripts\CreateDbLong.sql";
        private const string InsertDataForLongDataTestScriptFile = @"..\..\..\TestScripts\InsertDataLong.sql";

        private readonly ILoggerFactory _loggerFactory = new LoggerFactory();

        private readonly IDictionary<DbEngine, Tuple<string, string>> _hostAndDatabaseOptions =  new Dictionary<DbEngine, Tuple<string, string>>()
        {
            {DbEngine.SqlServer, new Tuple<string, string>(TestSqlServerHost, string.Format(TestSqlServerDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.SqLite, new Tuple<string, string>(string.Empty, string.Format(TestSqLiteDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.MySql, new Tuple<string, string>(TestMySqlHost, string.Format(TestMySqlDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.PostgresSql, new Tuple<string, string>(TestPostgresSqlHost, string.Format(TestPostgresSqlDatabase, Guid.NewGuid().ToString().Replace("-","")))}
        };
    }
}