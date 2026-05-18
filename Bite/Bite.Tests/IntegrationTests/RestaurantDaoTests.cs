using Bite.Dal.Ado;
using Bite.Dal.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.IntegrationTests;

public class RestaurantDaoTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly RestaurantDao dao;
    private readonly AdoTemplate template;

    public RestaurantDaoTests(DatabaseFixture fixture)
    {
        dao = new RestaurantDao(fixture.ConnectionFactory);
        template = new AdoTemplate(fixture.ConnectionFactory);
    }

    public async Task InitializeAsync() // beforeEach --> clear the Restaurant table
        => await template.ExecuteAsync("delete from Restaurant");

    public Task DisposeAsync() => Task.CompletedTask; // afterEach --> do nothing

    [Fact]
    public async Task FindAllAsync_EmptyTable_ReturnsEmptyList() 
    {
        var result = await dao.FindAllAsync();
        Assert.Empty(result);
    }
}
