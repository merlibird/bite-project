using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class OpeningHourSlotDaoTests : IAsyncLifetime
{
    private readonly OpeningHourSlotDao dao;
    private readonly RestaurantDao restaurantDao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public OpeningHourSlotDaoTests(DatabaseFixture fixture)
    {
        dao = new OpeningHourSlotDao(fixture.ConnectionFactory);
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
    public async Task FindByRestaurantIdAsync_TwoSlotsForRestaurant_ReturnsBothSlots()
    {
        int restaurantId = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId, dayOfWeek: 1));
        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId, dayOfWeek: 2));

        var result = await dao.FindByRestaurantIdAsync(restaurantId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_OnlyReturnsSlotsForGivenRestaurant()
    {
        int restaurantId1 = await SeedRestaurantAsync();
        int restaurantId2 = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId1, dayOfWeek: 1));
        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId2, dayOfWeek: 2));

        var result = await dao.FindByRestaurantIdAsync(restaurantId1);

        Assert.Single(result);
        Assert.Equal(1, result.First().DayOfWeek);
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        int restaurantId = await SeedRestaurantAsync();
        int dayOfWeek = 3; // Mittwoch
        var openTime = new TimeSpan(9, 0, 0);
        var closeTime = new TimeSpan(22, 30, 0);

        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId, dayOfWeek, openTime, closeTime));

        var result = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();

        Assert.Equal(restaurantId, result.RestaurantId);
        Assert.Equal(dayOfWeek, result.DayOfWeek);
        Assert.Equal(openTime, result.OpenTime);
        Assert.Equal(closeTime, result.CloseTime);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidOpeningHourSlot_ReturnsNewId()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeOpeningHourSlot(restaurantId));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoOpeningHourSlots_ReturnsDifferentIds()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id1 = await dao.InsertAsync(MakeOpeningHourSlot(restaurantId, dayOfWeek: 1));
        var id2 = await dao.InsertAsync(MakeOpeningHourSlot(restaurantId, dayOfWeek: 2));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static OpeningHourSlot MakeOpeningHourSlot(
        int restaurantId,
        int dayOfWeek = 1,
        TimeSpan? openTime = null,
        TimeSpan? closeTime = null) =>
            new OpeningHourSlot(
                0,
                restaurantId,
                dayOfWeek,
                openTime ?? new TimeSpan(9, 0, 0),
                closeTime ?? new TimeSpan(22, 0, 0));

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