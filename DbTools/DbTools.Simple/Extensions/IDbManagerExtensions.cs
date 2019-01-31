using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Utils;

namespace DbTools.Simple.Extensions
{
    public static class CommonDbManagerExtensions
    {
        /// <summary>
        ///     Creates Database for specified dbEngine and init it with scripts in order from index 0 to Count - 1
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="connectionString"> connection string to specified database </param>
        /// <param name="scripts"> a lists of file with sql scripts to init created database </param>
        /// <returns>
        ///     True if database was created and script were successfully executed
        /// </returns>
        public static bool Create(this IDbManager dbManager, string connectionString, IList<string> scripts)
        {
            if (dbManager == null || scripts.Any(s => !File.Exists(Path.GetFullPath(s))))
                return false;
            bool result = dbManager.CreateDatabase(connectionString, true);
            if (!result)
                return false;
            return InitImpl(dbManager, connectionString, scripts);
        }
        
        /// <summary>
        ///     Creates Database for specified dbEngine and init it with scripts in order from index 0 to Count - 1
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="dbEngine"> Db engine type </param>
        /// <param name="options"> dictionary of options to build connection string (see DbParametersKey for Key values) </param>
        /// <param name="scripts"> a lists of file with sql scripts to init created database </param>
        /// <returns>
        ///     Connection string of created database
        /// </returns>
        public static string Create(this IDbManager dbManager, DbEngine dbEngine, IDictionary<string, string> options, 
                                    IList<string> scripts)
        {
            if (dbManager == null || scripts.Any(s => !File.Exists(Path.GetFullPath(s))))
                return null;
            string connectionString = ConnectionStringBuilder.Build(dbEngine, options);
            bool result = dbManager.CreateDatabase(connectionString, true);
            if (!result)
                return null;
            return dbManager.Init(dbEngine, options, scripts);
        }
        
        /// <summary>
        ///     Creates Database for specified dbEngine and init it with scripts in order from index 0 to Count - 1 with
        ///     typical options to build connection string (host, database, user & password)
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="dbEngine"> Db engine type </param>
        /// <param name="host"> Database engine hostname i.e. localhost </param>
        /// <param name="database"> Creating database name </param>
        /// <param name="integratedSecurity"> Use built-in system authentication (typically Win authentication for SQL Server) </param>
        /// <param name="user"> Username if integrated security is not used </param>
        /// <param name="password"> User password </param>
        /// <param name="scripts"> List of scripts to init after create </param>
        /// <returns>
        ///     Connection string of created database
        /// </returns>
        public static string Create(this IDbManager dbManager, DbEngine dbEngine, 
                                    string host, string database, bool integratedSecurity, string user, string password, 
                                    IList<string> scripts)
        {
            if (dbManager == null || scripts.Any(s => !File.Exists(Path.GetFullPath(s))))
                return null;
            IDictionary<string, string> options = new Dictionary<string, string>()
            {
                {DbParametersKeys.HostKey, host},
                {DbParametersKeys.DatabaseKey, database},
                {DbParametersKeys.UseIntegratedSecurityKey, integratedSecurity.ToString()},
                {DbParametersKeys.LoginKey, user},
                {DbParametersKeys.PasswordKey, password}
            };
            string connectionString = ConnectionStringBuilder.Build(dbEngine, options);
            bool result = dbManager.CreateDatabase(connectionString, true);
            if (!result)
                return null;
            return dbManager.Init(connectionString, scripts) ? connectionString : null;
        }

        /// <summary>
        ///     Init created database (execution of scripts in order from 0 index to Count - 1)
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions)</param>
        /// <param name="connectionString"> connection string to specified database </param>
        /// <param name="scripts"> List of scripts to init database </param>
        /// <returns></returns>
        public static bool Init(this IDbManager dbManager, string connectionString, IList<string> scripts)
        {
            if (dbManager == null || scripts.Any(s => !File.Exists(Path.GetFullPath(s))))
                return false;
            return InitImpl(dbManager, connectionString, scripts);         
        }
        
        /// <summary>
        ///     Init previously created database (execution of scripts in order from 0 index to Count - 1)
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="dbEngine"> Db engine type </param>
        /// <param name="options"></param>
        /// <param name="scripts"> List of scripts to init database </param>
        /// <returns> connection string if script execution was successful </returns>
        public static string Init(this IDbManager dbManager, DbEngine dbEngine, IDictionary<string, string> options, 
                                  IList<string> scripts)
        {
            string connectionString = ConnectionStringBuilder.Build(dbEngine, options);
            bool result = InitImpl(dbManager, connectionString, scripts);
            return result ? connectionString : null;
        }

        /// <summary>
        ///     Init previously created database (execution of scripts in order from 0 index to Count - 1)
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="dbEngine"> Db engine type </param>
        /// <param name="host"> Database engine hostname i.e. localhost </param>
        /// <param name="database"> Creating database name </param>
        /// <param name="integratedSecurity"> Use built-in system authentication (typically Win authentication for SQL Server) </param>
        /// <param name="user"> Username if integrated security is not used </param>
        /// <param name="password"> User password </param>
        /// <param name="scripts"> List of scripts to init database </param>
        /// <returns></returns>
        public static string Init(this IDbManager dbManager, DbEngine dbEngine,
                                  string host, string database, bool integratedSecurity, string user, string password,
                                  IList<string> scripts)
        {
            IDictionary<string, string> options = new Dictionary<string, string>()
            {
                {DbParametersKeys.HostKey, host},
                {DbParametersKeys.DatabaseKey, database},
                {DbParametersKeys.UseIntegratedSecurityKey, integratedSecurity.ToString()},
                {DbParametersKeys.LoginKey, user},
                {DbParametersKeys.PasswordKey, password}
            };
            string connectionString = ConnectionStringBuilder.Build(dbEngine, options);
            bool result = InitImpl(dbManager, connectionString, scripts);
            return result ? connectionString : null;
        }

        private static bool InitImpl(IDbManager dbManager, string connectionString, IList<string> scripts)
        {
            bool result = true;
            foreach (string script in scripts)
            {
                string scriptContent = File.ReadAllText(Path.GetFullPath(script));
                result &= dbManager.ExecuteNonQuery(connectionString, scriptContent);
            }

            return result;
        }
    }
}