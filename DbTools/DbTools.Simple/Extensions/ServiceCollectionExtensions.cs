using System;
using System.Runtime.CompilerServices;
using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbTools.Simple.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbManager(this ServiceCollection services, DbEngine engine)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            services.AddScoped<IDbManager>(_ => DbManagerFactory.Create(engine, loggerFactory));
        }
    }
}