using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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
        await template.ExecuteAsync("delete from MenuItem", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuCategory", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Restaurant", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Address", Array.Empty<QueryParameter>());
    }

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
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingRestaurant_ReturnsTrue()
    {
        int addressId = await SeedAddressAsync();
        var id = await dao.InsertAsync(MakeRestaurant("Altes Gasthaus", addressId));
        var restaurant = await dao.FindByIdAsync(id);

        var result = await dao.UpdateAsync(restaurant!);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int addressId = await SeedAddressAsync();
        // not using MakeRestaurant() to avoid inserting a new restaurant with id=0
        var ghost = new Restaurant(69420, "Ghost Restaurant", addressId, "https://ghost.example.com/webhook");

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingRestaurant_PersistsChanges()
    {
        int addressId = await SeedAddressAsync();
        string updatedName = "Neues Gasthaus";

        var id = await dao.InsertAsync(MakeRestaurant("Altes Gasthaus", addressId));
        var restaurant = await dao.FindByIdAsync(id);
        restaurant!.Name = updatedName;

        await dao.UpdateAsync(restaurant);

        var updated = await dao.FindByIdAsync(id);

        Assert.Equal(updatedName, updated!.Name);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        int addressId = await SeedAddressAsync();
        var id = await dao.InsertAsync(MakeRestaurant("Zum Löwen", addressId));
        var restaurant = await dao.FindByIdAsync(id);

        var result = await dao.DeleteAsync(restaurant!.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CanNoLongerBeFound()
    {
        int addressId = await SeedAddressAsync();
        var id = await dao.InsertAsync(MakeRestaurant("Zum Löwen", addressId));
        await dao.DeleteAsync(id);

        var result = await dao.FindByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var result = await dao.DeleteAsync(69420);
        Assert.False(result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Restaurant MakeRestaurant(
        string name,
        int addressId,
        string webhookUrl = "example.com/webhook",
        string? titleImagePath = null) =>
            new Restaurant(0, name, addressId, webhookUrl, titleImagePath);

    private async Task<int> SeedAddressAsync() =>
        await addressDao.InsertAsync(
            new Address(0, "Hauptstraße", "1", "4040", "Linz", "Austria", 48.3, 14.2, null));
}
