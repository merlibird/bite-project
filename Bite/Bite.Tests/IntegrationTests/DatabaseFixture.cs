using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    public IConnectionFactory ConnectionFactory { get; private set; } = null!;

    // beforeAll --> initialize database connection
    public async Task InitializeAsync() 
    {
        var configuration = ConfigurationUtil.GetConfiguration();
        ConnectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "BiteDbConnection", "ProviderName");
    }

    public Task DisposeAsync() => Task.CompletedTask; 
}
