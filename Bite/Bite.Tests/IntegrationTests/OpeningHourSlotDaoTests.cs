using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

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
        await template.ExecuteAsync("delete from OpeningHourSlot", Array.Empty<QueryParameter>());
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
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingOpeningHourSlot_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId));
        var slot = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();

        var result = await dao.UpdateAsync(slot);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int restaurantId = await SeedRestaurantAsync();
        // not using MakeOpeningHourSlot() to avoid inserting a new slot with id=0
        var ghost = new OpeningHourSlot(69420, restaurantId, 1, new TimeSpan(9, 0, 0), new TimeSpan(22, 0, 0));

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingOpeningHourSlot_PersistsChanges()
    {
        int restaurantId = await SeedRestaurantAsync();
        var updatedCloseTime = new TimeSpan(23, 59, 0);

        await dao.InsertAsync(MakeOpeningHourSlot(restaurantId));
        var slot = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        slot.CloseTime = updatedCloseTime;

        await dao.UpdateAsync(slot);

        var updated = (await dao.FindByRestaurantIdAsync(restaurantId)).Single();
        Assert.Equal(updatedCloseTime, updated.CloseTime);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeOpeningHourSlot(restaurantId));

        var result = await dao.DeleteAsync(id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CanNoLongerBeFound()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeOpeningHourSlot(restaurantId));
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