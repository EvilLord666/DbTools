using DbTools.Core;
using DbTools.Core.Managers;
using DbTools.Simple.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DbTools.Simple.Tests.Extensions
{
    public class TestServiceCollectionExtensions
    {
        public TestServiceCollectionExtensions()
        {
            _services.AddLogging();
            _services.AddDbManager(DbEngine.SqlServer);
        }

        [Fact]
        public void TestAddDbManager()
        {
            ServiceProvider serviceProvider = _services.BuildServiceProvider();
            IDbManager dbManager = serviceProvider.GetService<IDbManager>();
            Assert.NotNull(dbManager);
        }
        
        private readonly ServiceCollection _services = new ServiceCollection();
    }
}