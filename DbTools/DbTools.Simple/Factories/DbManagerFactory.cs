using System;
using System.Data;
using System.Data.Common;
using DbTools.Core;
using DbTools.Core.Managers;
using System.Data.SqlClient;
using System.Data.SQLite;
using DbTools.Simple.Managers;
using Microsoft.Extensions.Logging;

namespace DbTools.Simple.Factories
{
    public static class DbManagerFactory
    {
        public static IDbManager Create(DbEngine dbEngine, ILoggerFactory loggerFactory)
        {
            return new CommonDbManager(dbEngine, loggerFactory.CreateLogger<CommonDbManager>());
        }
    }
}