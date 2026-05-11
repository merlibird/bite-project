using Bite.Dal.Ado;
using Bite.Dal.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public string ConnectionString { get; }
        public DatabaseConfig DbConfig { get; }  // <-- neu: wird von RestaurantDaoTests benötigt

        public DatabaseFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            DbConfig = new DatabaseConfig(config);
            ConnectionString = DbConfig.TargetConnectionString;

            var setup = new DatabaseSetup(DbConfig);
            setup.InitializeDatabase();

            var filler = new DataFillerClass(DbConfig);
            filler.FillTestData();
        }

        public void Dispose() { /* optional: Cleanup */ }
    }
}
