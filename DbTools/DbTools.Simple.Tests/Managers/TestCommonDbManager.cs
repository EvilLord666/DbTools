using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Factories;
using DbTools.Simple.Utils;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DbTools.Simple.Tests.Managers
{
    public class TestCommonDbManager
    {
        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "")]
        [InlineData(DbEngine.SqlServer, true, null, null)]
        [InlineData(DbEngine.SqLite, true, null, null)]
        [InlineData(DbEngine.MySql, false, "developer", "123")]
        [InlineData(DbEngine.PostgresSql, false, "developer", "123")]
        public void TestCreateAndDropDb(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password)
        {
            IDbManager dbManager = CreateTestDbManager(dbEngine);
            string connectionString = BuildConnectionString(dbEngine, useIntegratedSecurity, userName, password);
            dbManager.CreateDatabase(connectionString, true);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, true);
            dbManager.DropDatabase(connectionString);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, false);
        }

        /// <summary>
        ///     Test execute non-query (structure creation script : one table for compatibility across databases engines
        ///     do not execute this tests parallel, there are could be a problems with it.
        /// </summary>
        /// <param name="dbEngine"> one of enum value corresponding to proper db engine </param>
        /// <param name="useIntegratedSecurity"> affects only on SQL Server to use Win Authentication </param>
        /// <param name="userName"> user name if non sql server and useIntegratedSecurity is false </param>
        /// <param name="password"> user password (my test databases have password 123) </param>
        /// <param name="isAsync"> indicator to use async IDbManager interface or sync </param>
        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "", false)]
        [InlineData(DbEngine.SqlServer, true, "", "", true)]
        [InlineData(DbEngine.SqlServer, true, null, null, true)]
        [InlineData(DbEngine.SqlServer, true, null, null, false)]
        [InlineData(DbEngine.SqLite, true, null, null, false)]
        [InlineData(DbEngine.SqLite, true, null, null, true)]
        [InlineData(DbEngine.MySql, false, "developer", "123", false)]
        [InlineData(DbEngine.MySql, false, "developer", "123", true)]
        [InlineData(DbEngine.PostgresSql, false, "developer", "123", false)] 
        [InlineData(DbEngine.PostgresSql, false, "developer", "123", true)]
        public void TestExecuteNonQuery(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password, bool isAsync)
        {
            IDbManager dbManager = CreateTestDbManager(dbEngine);
            string connectionString = BuildConnectionString(dbEngine, useIntegratedSecurity, userName, password);
            dbManager.CreateDatabase(connectionString, true);
            string cmd = File.ReadAllText(Path.GetFullPath(CreateStructureScriptFile));
            ExecuteScriptAndCheck(dbManager, connectionString, cmd, isAsync);
            dbManager.DropDatabase(connectionString);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, false);
        }

        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "", false)]
        [InlineData(DbEngine.SqlServer, true, "", "", true)]
        [InlineData(DbEngine.SqLite, true, null, null, false)] // temporarily disabled due to some bad resource release
        [InlineData(DbEngine.SqLite, true, null, null, true)]
        [InlineData(DbEngine.MySql, false, "developer", "123", false)]
        [InlineData(DbEngine.MySql, false, "developer", "123", true)]
        [InlineData(DbEngine.PostgresSql, false, "developer", "123", false)] 
        [InlineData(DbEngine.PostgresSql, false, "developer", "123", true)]
        public void TestExecuteReader(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password, bool isAsync)
        {
            IDbManager dbManager = CreateTestDbManager(dbEngine);
            string connectionString = BuildConnectionString(dbEngine, useIntegratedSecurity, userName, password);
            dbManager.CreateDatabase(connectionString, true);
            string createTablesCmd = File.ReadAllText(Path.GetFullPath(CreateStructureScriptFile));
            string insertDataCmd = File.ReadAllText(Path.GetFullPath(InsertDataScriptFile));
            ExecuteScriptAndCheck(dbManager, connectionString, createTablesCmd, isAsync);
            ExecuteScriptAndCheck(dbManager, connectionString, insertDataCmd, isAsync);
            IList<object[]> actualData = new List<object[]>();
            const int numberOfColumns = 3;
            object[] row = new object[numberOfColumns];
            if (isAsync)
            {
                Task<Tuple<DbDataReader, DbConnection>> getReaderTask = dbManager.ExecuteDbReaderAsync(connectionString, SelectCitiesQuery);
                getReaderTask.Wait();
                DbDataReader asyncReader = getReaderTask.Result.Item1;
                
                Task<bool> readTask = asyncReader.ReadAsync();
                readTask.Wait();
                
                while (readTask.Result)
                {
                    for (int i = 0; i < numberOfColumns; i++)
                        row[i] = asyncReader.GetValue(i);
                    actualData.Add(row);
                    readTask = asyncReader.ReadAsync();
                    readTask.Wait();
                }
                asyncReader.Close();
                asyncReader.Dispose();
                getReaderTask.Result.Item2.Close();
            }
            else
            {
                Tuple<IDataReader, IDbConnection> reader = dbManager.ExecuteDbReader(connectionString, SelectCitiesQuery);
                while (reader.Item1.Read())
                {
                    for (int i = 0; i < numberOfColumns; i++)
                        row[i] = reader.Item1.GetValue(i);
                    actualData.Add(row);
                }
                reader.Item1.Close();
                reader.Item1.Dispose();
                reader.Item2.Close();
            }
            Assert.Equal(8, actualData.Count);  // indicator tests
            dbManager.DropDatabase(connectionString);
            CheckDatabaseExists(dbManager, dbEngine, connectionString, false);
        }

        [Theory]
        [InlineData(DbEngine.MySql, false, "developer", "123", true)]
        [InlineData(DbEngine.MySql, false, "developer", "123", false)]
        private void TestCreateLongData(DbEngine dbEngine, bool useIntegratedSecurity, string userName, string password, bool isAsync)
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

        private void CheckDatabaseExists(IDbManager dbManager, DbEngine dbEngine, string connectionString, bool expected)
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
        }

        private const string TestMySqlHost = "localhost";
        private const string TestSqlServerHost = @"(localdb)\mssqllocaldb";
        private const string TestPostgresSqlHost = "localhost";
        
        private const string TestSqlServerDatabase = "SQLServerTestDb_{0}";
        private const string TestSqLiteDatabase = "SqLiteTestDb_{0}.sqlite";     
        private const string TestMySqlDatabase = "MySqlTestDb_{0}";    
        private const string TestPostgresSqlDatabase = "PostgresTestDb_{0}";

        private const string SelectDatabaseTemplate = "SELECT {0} FROM {1} WHERE {0}={2};";
        private const string SelectCitiesQuery = "SELECT Id, Name, RegionId FROM City";
        private const string CreateStructureScriptFile = @"..\..\..\TestScripts\CreateDb.sql";
        private const string InsertDataScriptFile = @"..\..\..\TestScripts\InsertData.sql";
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