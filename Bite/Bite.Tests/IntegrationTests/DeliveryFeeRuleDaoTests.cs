using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class DeliveryFeeRuleDaoTests : IAsyncLifetime
{
    private readonly DeliveryFeeRuleDao dao;
    private readonly DeliveryZoneDao deliveryZoneDao;
    private readonly RestaurantDao restaurantDao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public DeliveryFeeRuleDaoTests(DatabaseFixture fixture)
    {
        dao = new DeliveryFeeRuleDao(fixture.ConnectionFactory);
        deliveryZoneDao = new DeliveryZoneDao(fixture.ConnectionFactory);
        restaurantDao = new RestaurantDao(fixture.ConnectionFactory);
        addressDao = new AddressDao(fixture.ConnectionFactory);
        template = new AdoTemplate(fixture.ConnectionFactory);
    }

    // beforeEach --> clear all tables in FK-safe order
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
    public async Task FindByRestaurantIdAsync_TwoRulesForRestaurant_ReturnsBothRules()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 20.00m));
        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 50.00m));

        var result = await dao.FindByRestaurantIdAsync(restaurantId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_OnlyReturnsRulesForGivenRestaurant()
    {
        int restaurantId1 = await SeedRestaurantAsync();
        int restaurantId2 = await SeedRestaurantAsync();
        int zoneId1 = await SeedDeliveryZoneAsync(restaurantId1);
        int zoneId2 = await SeedDeliveryZoneAsync(restaurantId2);

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId1, maxOrderValue: 20.00m, deliveryFee: 2.99m));
        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId2, maxOrderValue: 50.00m, deliveryFee: 4.99m));

        var result = await dao.FindByRestaurantIdAsync(restaurantId1);

        Assert.Single(result);
        Assert.Equal(2.99m, result.First().DeliveryFee);
    }

    // -------------------------------------------------------------------------
    // FindByRestaurantIdAndZoneIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByRestaurantIdAndZoneIdAsync_NonExistingIds_ReturnsEmptyList()
    {
        var result = await dao.FindByRestaurantIdAndZoneIdAsync(69420, 69420);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindByRestaurantIdAndZoneIdAsync_TwoRulesForZone_ReturnsBothRules()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 20.00m));
        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 50.00m));

        var result = await dao.FindByRestaurantIdAndZoneIdAsync(restaurantId, zoneId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindByRestaurantIdAndZoneIdAsync_OnlyReturnsRulesForGivenZone()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId1 = await SeedDeliveryZoneAsync(restaurantId);
        int zoneId2 = await SeedDeliveryZoneAsync(restaurantId);

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId1, maxOrderValue: 20.00m, deliveryFee: 2.99m));
        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId2, maxOrderValue: 50.00m, deliveryFee: 4.99m));

        var result = await dao.FindByRestaurantIdAndZoneIdAsync(restaurantId, zoneId1);

        Assert.Single(result);
        Assert.Equal(2.99m, result.First().DeliveryFee);
    }

    [Fact]
    public async Task FindByRestaurantIdAndZoneIdAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        decimal maxOrderValue = 30.00m;
        decimal deliveryFee = 3.49m;

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue, deliveryFee));

        var result = (await dao.FindByRestaurantIdAndZoneIdAsync(restaurantId, zoneId)).Single();

        Assert.Equal(zoneId, result.DeliveryZoneId);
        Assert.Equal(maxOrderValue, result.MaxOrderValue);
        Assert.Equal(deliveryFee, result.DeliveryFee);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidDeliveryFeeRule_ReturnsNewId()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        var id = await dao.InsertAsync(MakeDeliveryFeeRule(zoneId));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoDeliveryFeeRules_ReturnsDifferentIds()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        var id1 = await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 20.00m));
        var id2 = await dao.InsertAsync(MakeDeliveryFeeRule(zoneId, maxOrderValue: 50.00m));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingDeliveryFeeRule_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId));
        var rule = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();

        var result = await dao.UpdateAsync(rule);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        // not using MakeDeliveryFeeRule() to avoid inserting a new rule with id=0
        var ghost = new DeliveryFeeRule(69420, zoneId, 20.00m, 2.99m);

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingDeliveryFeeRule_PersistsChanges()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        decimal updatedDeliveryFee = 9.99m;

        await dao.InsertAsync(MakeDeliveryFeeRule(zoneId));
        var rule = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        rule.DeliveryFee = updatedDeliveryFee;

        await dao.UpdateAsync(rule);

        var updated = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        Assert.Equal(updatedDeliveryFee, updated.DeliveryFee);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        var id = await dao.InsertAsync(MakeDeliveryFeeRule(zoneId));

        var result = await dao.DeleteAsync(id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CanNoLongerBeFound()
    {
        int restaurantId = await SeedRestaurantAsync();
        int zoneId = await SeedDeliveryZoneAsync(restaurantId);
        var id = await dao.InsertAsync(MakeDeliveryFeeRule(zoneId));
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

    private static DeliveryFeeRule MakeDeliveryFeeRule(
        int deliveryZoneId,
        decimal maxOrderValue = 30.00m,
        decimal deliveryFee = 2.99m) =>
            new DeliveryFeeRule(0, deliveryZoneId, maxOrderValue, deliveryFee);

    private async Task<int> SeedAddressAsync() =>
        await addressDao.InsertAsync(
            new Address(0, "Hauptstraße", "1", "4040", "Linz", "Austria", 48.3, 14.2, null));

    private async Task<int> SeedRestaurantAsync()
    {
        int addressId = await SeedAddressAsync();
        return await restaurantDao.InsertAsync(
            new Restaurant(0, "Testrestaurant", addressId, "https://example.com/webhook", Guid.NewGuid().ToString()));
    }

    private async Task<int> SeedDeliveryZoneAsync(int restaurantId) =>
        await deliveryZoneDao.InsertAsync(
            new DeliveryZone(0, restaurantId, 10.00m, 5.0));
}