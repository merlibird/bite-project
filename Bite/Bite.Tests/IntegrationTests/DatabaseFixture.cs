using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    public IConnectionFactory ConnectionFactory { get; private set; } = null!;

    public async Task InitializeAsync() // beforeAll --> initialize database connection
    {
        var configuration = ConfigurationUtil.GetConfiguration();
        ConnectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "BiteDbConnection", "ProviderName");
    }

    public Task DisposeAsync() => Task.CompletedTask; // afterAll --> do nothing
}
