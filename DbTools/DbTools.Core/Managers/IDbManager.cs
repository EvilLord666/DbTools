using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbTools.Core.Managers
{
    public interface IDbManager
    {
        bool CreateDatabase(string connectionString, bool dropIfExists);
        bool DropDatabase(string connectionString);
        bool ExecuteNonQuery(IDbCommand command);
        bool ExecuteNonQuery(string connectionString, string cmdText); 
        Task<bool> ExecuteNonQueryAsync(DbCommand command);
        Task<bool> ExecuteNonQueryAsync(string connectionString, string cmdText);
        IDataReader ExecuteDbReader(IDbCommand command);
        Tuple<IDataReader, IDbConnection> ExecuteDbReader(string connectionString, string cmdText);
        Task<DbDataReader> ExecuteDbReaderAsync(DbCommand command);
        Task<Tuple<DbDataReader, DbConnection>> ExecuteDbReaderAsync(string connectionString, string cmdText);
    }
}