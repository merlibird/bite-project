using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
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

    // beforeEach --> clear the Restaurant table
    public async Task InitializeAsync()
        => await template.ExecuteAsync("delete from Restaurant", Array.Empty<QueryParameter>());

    // afterEach --> do nothing
    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // FindAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindAllAsync_EmptyTable_ReturnsEmptyList() 
    {
        var result = await dao.FindAllAsync();
        Assert.Empty(result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Restaurant MakeRestaurant(
        string name,
        int addressId = 1,
        string? webhookUrl = null,
        string? titleImagePath = null) =>
            new Restaurant(0, name, addressId, webhookUrl, titleImagePath);
}
