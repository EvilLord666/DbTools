using System.Collections.Generic;
using DbTools.Core;
using DbTools.Core.Managers;

namespace DbTools.Simple.Extensions
{
    public static class CommonDbManagerExtensions
    {
        /// <summary>
        ///     Creates Database for specified dbEngine and init it with scripts in order from index 0 to Count - 1
        /// </summary>
        /// <param name="dbManager"> An instance of IDbManager (see DbManagerFactory, or ServiceCollectionExtensions) </param>
        /// <param name="dbEngine"> Db engine type </param>
        /// <param name="connectionString"> connection string to specified database </param>
        /// <param name="scripts"> a lists of file with sql scripts to init created database </param>
        /// <returns>
        ///     True if database was created and script were successfully executed
        /// </returns>
        public static bool Create(this IDbManager dbManager, DbEngine dbEngine, string connectionString, IList<string> scripts)
        {
            return false;
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
        public static string Create(this IDbManager dbManager, DbEngine dbEngine, IDictionary<string, string> options, IList<string> scripts)
        {
            return null;
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
            return null;
        }

        /// <summary>
        ///     Init created database (execution of scripts in order from 0 index to Count - 1)
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="dbEngine"></param>
        /// <param name="connectionString"></param>
        /// <param name="scripts"></param>
        /// <returns></returns>
        public static bool Init(this IDbManager dbManager, DbEngine dbEngine, string connectionString, IList<string> scripts)
        {
            return false;
        }
        
        /// <summary>
        ///     Init created database (execution of scripts in order from 0 index to Count - 1)
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="dbEngine"></param>
        /// <param name="options"></param>
        /// <param name="scripts"></param>
        /// <returns></returns>
        public static bool Init(this IDbManager dbManager, DbEngine dbEngine, IDictionary<string, string> options, 
                                IList<string> scripts)
        {
            return false;
        }
    }
}