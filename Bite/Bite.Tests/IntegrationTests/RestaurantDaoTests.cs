using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class RestaurantDaoTests : IAsyncLifetime
{
    private readonly RestaurantDao dao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public RestaurantDaoTests(DatabaseFixture fixture)
    {
        dao = new RestaurantDao(fixture.ConnectionFactory);
        addressDao = new AddressDao(fixture.ConnectionFactory);
        template = new AdoTemplate(fixture.ConnectionFactory);
    }

    // beforeEach --> clear tables and insert a valid address for FK references
    public async Task InitializeAsync()
    {
        await template.ExecuteAsync("delete from OrderStatusToken", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from OrderItem", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from CustomerOrder", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuItemMenuCategory", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuItem", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuCategory", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from OpeningHourSlot", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from DeliveryFeeRule", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from DeliveryZone", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Restaurant", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Address", Array.Empty<QueryParameter>());
    }

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

    [Fact]
    public async Task FindAllAsync_TwoRestaurantsInserted_ReturnsBothRestaurants()
    {
        int addressId = await SeedAddressAsync();

        await dao.InsertAsync(MakeRestaurant("Zum goldenen Hirschen", addressId));
        await dao.InsertAsync(MakeRestaurant("Pizzeria Napoli", addressId));

        var result = await dao.FindAllAsync();

        Assert.Equal(2, result.Count());
    }

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await dao.FindByIdAsync(69420);
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        int addressId = await SeedAddressAsync();
        string name = "Zum goldenen Hirschen";
        string webhookUrl = "https://example.com/webhook";
        string titleImagePath = "/images/hirschen.jpg";

        var id = await dao.InsertAsync(MakeRestaurant(name, addressId, webhookUrl, titleImagePath));

        var result = await dao.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(addressId, result.AddressId);
        Assert.Equal(webhookUrl, result.WebhookUrl);
        Assert.Equal(titleImagePath, result.TitleImagePath);
    }

    [Fact]
    public async Task FindByIdAsync_NullableFieldNotSet_ReturnsNull()
    {
        int addressId = await SeedAddressAsync();
        var id = await dao.InsertAsync(MakeRestaurant("Burgerhaus", addressId));

        var result = await dao.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Null(result.TitleImagePath);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidRestaurant_ReturnsNewId()
    {
        int addressId = await SeedAddressAsync();
        var id = await dao.InsertAsync(MakeRestaurant("Sushi Garden", addressId));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoRestaurants_ReturnsDifferentIds()
    {
        int addressId = await SeedAddressAsync();
        var id1 = await dao.InsertAsync(MakeRestaurant("Trattoria Roma", addressId));
        var id2 = await dao.InsertAsync(MakeRestaurant("Wiener Schnitzelhaus", addressId));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Restaurant MakeRestaurant(
        string name,
        int addressId,
        string webhookUrl = "example.com/webhook",
        string? titleImagePath = null,
        string? apiKey = null) =>
            new Restaurant(0, name, addressId, webhookUrl, apiKey ?? Guid.NewGuid().ToString(), titleImagePath);

    private async Task<int> SeedAddressAsync() =>
        await addressDao.InsertAsync(
            new Address(0, "Hauptstraße", "1", "4040", "Linz", "Austria", 48.3, 14.2, null));
}
