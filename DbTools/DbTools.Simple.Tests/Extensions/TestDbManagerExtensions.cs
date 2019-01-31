using System;
using System.Collections.Generic;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Extensions;
using DbTools.Simple.Factories;
using DbTools.Simple.Utils;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DbTools.Simple.Tests.Extensions
{
    public class TestDbManagerExtensions
    {
        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "")]
        [InlineData(DbEngine.MySql, false, "root", "123")]
        [InlineData(DbEngine.SqLite, false, "", "")]
        [InlineData(DbEngine.PostgresSql, false, "postgres", "123")]
        public void TestCreate(DbEngine dbEngine, bool integratedSecurity, string userName, string password)
        {
            IDbManager dbManager = DbManagerFactory.Create(dbEngine, _loggerFactory);
            Tuple<string, string> hostAndName = _hostAndDatabaseOptions[dbEngine];
            string connectionString = dbManager.Create(dbEngine, hostAndName.Item1, hostAndName.Item2,
                                                       integratedSecurity, userName, password, _scripts);
            Assert.NotNull(connectionString);
            bool result = dbManager.DropDatabase(connectionString);
            Assert.True(result);
        }

        [Theory]
        [InlineData(DbEngine.SqlServer, true, "", "")]
        [InlineData(DbEngine.MySql, false, "root", "123")]
        [InlineData(DbEngine.SqLite, false, "", "")]
        [InlineData(DbEngine.PostgresSql, false, "postgres", "123")]
        public void TestInit(DbEngine dbEngine, bool integratedSecurity, string userName, string password)
        {
            IDbManager dbManager = DbManagerFactory.Create(dbEngine, _loggerFactory);
            Tuple<string, string> hostAndName = _hostAndDatabaseOptions[dbEngine];
            IDictionary<string, string> options = new Dictionary<string, string>()
            {
                {DbParametersKeys.HostKey, hostAndName.Item1},
                {DbParametersKeys.DatabaseKey, hostAndName.Item2},
                {DbParametersKeys.UseIntegratedSecurityKey, integratedSecurity.ToString()},
                {DbParametersKeys.LoginKey, userName},
                {DbParametersKeys.PasswordKey, password}
            };

            string connectionString = ConnectionStringBuilder.Build(dbEngine, options);
            bool result = dbManager.CreateDatabase(connectionString, true);
            Assert.True(result);
            result = dbManager.Init(connectionString, _scripts);
            Assert.True(result);
            result = dbManager.DropDatabase(connectionString);
            Assert.True(result);
        }

        private const string TestMySqlHost = "localhost";
        private const string TestSqlServerHost = @"(localdb)\mssqllocaldb";
        private const string TestPostgresSqlHost = "localhost";
        
        private const string TestSqlServerDatabase = "SQLServerTestDb_{0}";
        private const string TestSqLiteDatabase = "SqLiteTestDb_{0}.sqlite";     
        private const string TestMySqlDatabase = "MySqlTestDb_{0}";    
        private const string TestPostgresSqlDatabase = "PostgresTestDb_{0}";
        
        private readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        
        private readonly IDictionary<DbEngine, Tuple<string, string>> _hostAndDatabaseOptions =  new Dictionary<DbEngine, Tuple<string, string>>()
        {
            {DbEngine.SqlServer, new Tuple<string, string>(TestSqlServerHost, string.Format(TestSqlServerDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.SqLite, new Tuple<string, string>(string.Empty, string.Format(TestSqLiteDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.MySql, new Tuple<string, string>(TestMySqlHost, string.Format(TestMySqlDatabase, Guid.NewGuid().ToString().Replace("-","")))},
            {DbEngine.PostgresSql, new Tuple<string, string>(TestPostgresSqlHost, string.Format(TestPostgresSqlDatabase, Guid.NewGuid().ToString().Replace("-","")))}
        };

        private readonly IList<string> _scripts = new List<string>()
        {
            @"..\..\..\TestScripts\CreateDb.sql",
            @"..\..\..\TestScripts\InsertData.sql"
        };
    }
}