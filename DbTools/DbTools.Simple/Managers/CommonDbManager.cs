using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Factories;
using DbTools.Simple.Utils;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DbTools.Simple.Managers
{
    public class CommonDbManager : IDbManager
    {
        public CommonDbManager(DbEngine dbEngine, ILogger<CommonDbManager> logger)
        {
            _dbEngine = dbEngine;
            _logger = logger;
        }

        public bool CreateDatabase(string connectionString, bool dropIfExists)
        {
            try
            {
                if (dropIfExists)
                    DropDatabase(connectionString);
                string dbName = ConnectionStringHelper.GetDatabaseName(connectionString, _dbEngine);
                if (_dbEngine == DbEngine.SqLite)
                {
                    SQLiteConnection.CreateFile(dbName);
                    return true;
                }
                string createDbStatement = string.Format(CommonServerCreateDatabaseStatementTemplate, dbName);
                if (_dbEngine == DbEngine.SqlServer)
                    connectionString = ConnectionStringHelper.GetSqlServerMasterConnectionString(connectionString);
                if (_dbEngine == DbEngine.MySql)
                    connectionString = ConnectionStringHelper.GetMySqlSysDbConnectionString(connectionString);
                if (_dbEngine == DbEngine.PostgresSql)
                    connectionString = ConnectionStringHelper.GetPostgresSqlSysDbConnectionString(connectionString);

                return ExecuteStatement(connectionString, createDbStatement);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during database creation, exception {e}");
                return false;
            }

        }

        public bool DropDatabase(string connectionString)
        {
            try
            {
                string dbName = ConnectionStringHelper.GetDatabaseName(connectionString, _dbEngine);
                if (_dbEngine == DbEngine.SqLite)
                {
                    SQLiteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    string dbFile = Path.GetFullPath(dbName);
                    if (File.Exists(dbFile))
                        File.Delete(dbFile);
                    return true;
                }
                IList<string> dropSqlStatements = GetDropDatabaseStatement(dbName);
                if (_dbEngine == DbEngine.SqlServer)
                    connectionString = ConnectionStringHelper.GetSqlServerMasterConnectionString(connectionString);
                if (_dbEngine == DbEngine.PostgresSql)
                    connectionString = ConnectionStringHelper.GetPostgresSqlSysDbConnectionString(connectionString);

                bool result = true;
                foreach (string subCommand in dropSqlStatements)
                {
                    result &= ExecuteStatement(connectionString, subCommand);
                }
                if (_dbEngine == DbEngine.PostgresSql)
                    NpgsqlConnection.ClearAllPools();              // fixes issue with connection re-open

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during database drop, exception {e}");
                return false;
            }
        }

        /// <summary>
        ///      Method for database command execution without any result (UPDATE, DELETE or INSERT command)
        ///      This method works ONLY if connection was opened, otherwise it fails, see for example
        ///      ExecuteNonQuery(string connectionString, string cmdText)
        /// </summary>
        /// <param name="command"> Sql Command to execute (should be build using DbCommandFactory) </param>
        /// <returns>
        ///      True if command was executed without any errors, otherwise - false
        /// </returns>
        public bool ExecuteNonQuery(IDbCommand command)
        {
            bool result = true;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during execute non query, exception {e}");
                result = false;
            }
            finally
            {
                command.Dispose();
            }

            return result;
        }

        /// <summary>
        ///      Full sql command execution in one call without manual connection open and close, constructing
        ///      proper instance of DbCommand
        /// </summary>
        /// <param name="connectionString"> Database connection string </param>
        /// <param name="cmdText"> Script that should be executed </param>
        /// <returns>
        ///      True if execution was successful, otherwise - false
        /// </returns>
        public bool ExecuteNonQuery(string connectionString, string cmdText)
        {
            using (DbConnection connection = DbConnectionFactory.Create(_dbEngine, connectionString))
            {
                IDbCommand command = DbCommandFactory.Create(_dbEngine, connection, cmdText);
                connection.Open();
                bool result = ExecuteNonQuery(command as DbCommand);
                connection.Close();
                return result;
            }
        }

        public async Task<bool> ExecuteNonQueryAsync(DbCommand command)
        {
            bool result = true;
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during execute non query async, exception {e}");
                result = false;
            }
            finally
            {
                command.Dispose();
            }

            return result;
        }

        public async Task<bool> ExecuteNonQueryAsync(string connectionString, string cmdText)
        {
            using (DbConnection connection = DbConnectionFactory.Create(_dbEngine, connectionString))
            {
                IDbCommand command = DbCommandFactory.Create(_dbEngine, connection, cmdText);
                await connection.OpenAsync().ConfigureAwait(false);
                bool result = await ExecuteNonQueryAsync(command as DbCommand);
                connection.Close();
                return result;
            }
        }

        public IDataReader ExecuteDbReader(IDbCommand command)
        {
            IDataReader result = null;
            try
            {
                result = command.ExecuteReader(CommandBehavior.SequentialAccess);
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during Db Reader Execution: {e}");
                result = null;
            }

            return result;
        }

        public Tuple<IDataReader, IDbConnection> ExecuteDbReader(string connectionString, string cmdText)
        {
            IDbConnection connection = DbConnectionFactory.Create(_dbEngine, connectionString);
            IDbCommand command = DbCommandFactory.Create(_dbEngine, connection, cmdText);
            connection.Open();
            return new Tuple<IDataReader, IDbConnection>(ExecuteDbReader(command as DbCommand), connection);
        }

        public async Task<DbDataReader> ExecuteDbReaderAsync(DbCommand command)
        {
            DbDataReader result = null;
            try
            {
                result = await command.ExecuteReaderAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred during Async Db Reader Execution: {e}");
                result = null;
            }

            return result;
        }

        public async Task<Tuple<DbDataReader, DbConnection>> ExecuteDbReaderAsync(string connectionString, string cmdText)
        {
            DbConnection connection = DbConnectionFactory.Create(_dbEngine, connectionString);
            await connection.OpenAsync();
            IDbCommand command = DbCommandFactory.Create(_dbEngine, connection, cmdText);
            return new Tuple<DbDataReader, DbConnection>(await ExecuteDbReaderAsync(command as DbCommand), connection);
        }

        private bool ExecuteStatement(string connectionString, string statement)
        {
            bool result = true;
            using (IDbConnection connection = DbConnectionFactory.Create(_dbEngine, connectionString))
            {
                connection.Open();
                IDbCommand command = DbCommandFactory.Create(_dbEngine, connection, statement);
                result = ExecuteNonQuery(command);
                connection.Close();
                return result;
            }
        }

        private IList<string> GetDropDatabaseStatement(string dbName)
        {
            IList<string> dropCommands = new List<string>();
            if (_dbEngine == DbEngine.SqlServer)
                dropCommands.Add(string.Format(SqlServerDropDatabaseStatementTemplate, dbName));
            if (_dbEngine == DbEngine.SqLite)
                dropCommands.Add(string.Format(SqLiteDropDatabaseStatementTemplate, dbName));
            if (_dbEngine == DbEngine.MySql)
                dropCommands.Add(string.Format(MySqlDropDatabaseStatementTemplate, dbName));
            if (_dbEngine == DbEngine.PostgresSql)
            {
                string commandsPipeline = String.Format(PostgresSqlDropDatabaseStatementTemplate, dbName);
                string[] subCommands = commandsPipeline.Split(new char[]{';'});
                dropCommands = subCommands.Where(s => s.Length > 2).Select(s => s.EndsWith(";") ? s : $"{s};").ToList();
            }

            return dropCommands;
        }


        // create database statements
        private const string CommonServerCreateDatabaseStatementTemplate = "CREATE DATABASE {0};";
        //private const string MySqlCreateDatabaseStatementTemplate = "CREATE DATABASE {0} IF EXISTS;";
        // drop database statements
        private const string SqlServerDropDatabaseStatementTemplate = "ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];";
        private const string MySqlDropDatabaseStatementTemplate = "DROP DATABASE {0};";
        private const string SqLiteDropDatabaseStatementTemplate = "DETACH DATABASE {0};";
        private const string PostgresSqlDropDatabaseStatementTemplate = @"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname ='{0}' AND pid <> pg_backend_pid(); DROP DATABASE {0};";

        private readonly DbEngine _dbEngine;
        private readonly ILogger<CommonDbManager> _logger;
    }
}