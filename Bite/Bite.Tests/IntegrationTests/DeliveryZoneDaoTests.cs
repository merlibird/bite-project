using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class DeliveryZoneDaoTests : IAsyncLifetime
{
    private readonly DeliveryZoneDao dao;
    private readonly RestaurantDao restaurantDao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public DeliveryZoneDaoTests(DatabaseFixture fixture)
    {
        dao = new DeliveryZoneDao(fixture.ConnectionFactory);
        restaurantDao = new RestaurantDao(fixture.ConnectionFactory);
        addressDao = new AddressDao(fixture.ConnectionFactory);
        template = new AdoTemplate(fixture.ConnectionFactory);
    }

    // beforeEach --> clear all tables in FK-safe order
    public async Task InitializeAsync()
    {
        await template.ExecuteAsync("delete from DeliveryFeeRule", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from DeliveryZone", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Restaurant", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Address", Array.Empty<QueryParameter>());
    }

    // afterEach --> do nothing
    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // FindByRestaurantIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByRestaurantIdAsync_NonExistingRestaurantId_ReturnsEmptyList()
    {
        var result = await dao.FindByRestaurantIdAsync(69420);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_TwoZonesForRestaurant_ReturnsBothZones()
    {
        int restaurantId = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeDeliveryZone(restaurantId, maxDistance: 5.0));
        await dao.InsertAsync(MakeDeliveryZone(restaurantId, maxDistance: 10.0));

        var result = await dao.FindByRestaurantIdAsync(restaurantId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_OnlyReturnsZonesForGivenRestaurant()
    {
        int restaurantId1 = await SeedRestaurantAsync();
        int restaurantId2 = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeDeliveryZone(restaurantId1, maxDistance: 5.0));
        await dao.InsertAsync(MakeDeliveryZone(restaurantId2, maxDistance: 10.0));

        var result = await dao.FindByRestaurantIdAsync(restaurantId1);

        Assert.Single(result);
        Assert.Equal(5.0, result.First().MaxDistance);
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        int restaurantId = await SeedRestaurantAsync();
        decimal minOrderValue = 15.00m;
        double maxDistance = 7.5;

        await dao.InsertAsync(MakeDeliveryZone(restaurantId, minOrderValue, maxDistance));

        var result = await dao.FindByRestaurantIdAsync(restaurantId);
        var zone = result.Single();

        Assert.Equal(restaurantId, zone.RestaurantId);
        Assert.Equal(minOrderValue, zone.MinOrderValue);
        Assert.Equal(maxDistance, zone.MaxDistance);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidDeliveryZone_ReturnsNewId()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeDeliveryZone(restaurantId));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoDeliveryZones_ReturnsDifferentIds()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id1 = await dao.InsertAsync(MakeDeliveryZone(restaurantId, maxDistance: 5.0));
        var id2 = await dao.InsertAsync(MakeDeliveryZone(restaurantId, maxDistance: 10.0));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingDeliveryZone_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeDeliveryZone(restaurantId));
        var zone = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();

        var result = await dao.UpdateAsync(zone);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int restaurantId = await SeedRestaurantAsync();
        // not using MakeDeliveryZone() to avoid inserting a new zone with id=0
        var ghost = new DeliveryZone(69420, restaurantId, 10.00m, 5.0);

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingDeliveryZone_PersistsChanges()
    {
        int restaurantId = await SeedRestaurantAsync();
        double updatedMaxDistance = 99.9;

        await dao.InsertAsync(MakeDeliveryZone(restaurantId));
        var zone = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        zone.MaxDistance = updatedMaxDistance;

        await dao.UpdateAsync(zone);

        var updated = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        Assert.Equal(updatedMaxDistance, updated.MaxDistance);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeDeliveryZone(restaurantId));

        var result = await dao.DeleteAsync(id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CanNoLongerBeFound()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeDeliveryZone(restaurantId));
        await dao.DeleteAsync(id);

        var result = await dao.FindByRestaurantIdAsync(restaurantId);

        Assert.Empty(result);
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

    private static DeliveryZone MakeDeliveryZone(
        int restaurantId,
        decimal minOrderValue = 10.00m,
        double maxDistance = 5.0) =>
            new DeliveryZone(0, restaurantId, minOrderValue, maxDistance);

    private async Task<int> SeedAddressAsync() =>
        await addressDao.InsertAsync(
            new Address(0, "Hauptstraße", "1", "4040", "Linz", "Austria", 48.3, 14.2, null));

    private async Task<int> SeedRestaurantAsync()
    {
        int addressId = await SeedAddressAsync();
        return await restaurantDao.InsertAsync(
            new Restaurant(0, "Testrestaurant", addressId, "https://example.com/webhook", Guid.NewGuid().ToString()));
    }
}