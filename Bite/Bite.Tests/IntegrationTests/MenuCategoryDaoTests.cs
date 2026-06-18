using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class MenuCategoryDaoTests : IAsyncLifetime
{
    private readonly MenuCategoryDao dao;
    private readonly RestaurantDao restaurantDao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public MenuCategoryDaoTests(DatabaseFixture fixture)
    {
        dao = new MenuCategoryDao(fixture.ConnectionFactory);
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
    // FindAllByRestaurantIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindAllByRestaurantIdAsync_NonExistingRestaurantId_ReturnsEmptyList()
    {
        var result = await dao.FindAllByRestaurantIdAsync(69420);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindAllByRestaurantIdAsync_TwoCategoriesForRestaurant_ReturnsBothCategories()
    {
        int restaurantId = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId));
        await dao.InsertAsync(MakeMenuCategory("Hauptspeisen", restaurantId));

        var result = await dao.FindAllByRestaurantIdAsync(restaurantId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindAllByRestaurantIdAsync_OnlyReturnsCategoriesForGivenRestaurant()
    {
        int restaurantId1 = await SeedRestaurantAsync();
        int restaurantId2 = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId1));
        await dao.InsertAsync(MakeMenuCategory("Desserts", restaurantId2));

        var result = await dao.FindAllByRestaurantIdAsync(restaurantId1);

        Assert.Single(result);
        Assert.Equal("Vorspeisen", result.First().Name);
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
        int restaurantId = await SeedRestaurantAsync();
        string name = "Hauptspeisen";
        bool isActive = false;

        var id = await dao.InsertAsync(new MenuCategory(0, restaurantId, name, isActive));

        var result = await dao.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(restaurantId, result.RestaurantId);
        Assert.Equal(isActive, result.IsActive);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidMenuCategory_ReturnsNewId()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoMenuCategories_ReturnsDifferentIds()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id1 = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));
        var id2 = await dao.InsertAsync(MakeMenuCategory("Hauptspeisen", restaurantId, true));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingMenuCategory_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));
        var category = await dao.FindByIdAsync(id);

        var result = await dao.UpdateAsync(category!);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int restaurantId = await SeedRestaurantAsync();
        // not using MakeMenuCategory() to avoid inserting a new category with id=0
        var ghost = new MenuCategory(69420, restaurantId, "Ghost Category", true);

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMenuCategory_PersistsChanges()
    {
        int restaurantId = await SeedRestaurantAsync();
        string updatedName = "Desserts";
        bool updatedActive = false;

        var id = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));
        var category = await dao.FindByIdAsync(id);
        category!.Name = updatedName;
        category!.IsActive = updatedActive;

        await dao.UpdateAsync(category);

        var updated = await dao.FindByIdAsync(id);
        Assert.Equal(updatedName, updated!.Name);
        Assert.Equal(updatedActive, updated!.IsActive);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));
        var category = await dao.FindByIdAsync(id);

        var result = await dao.DeleteAsync(category!.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CanNoLongerBeFound()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuCategory("Vorspeisen", restaurantId, true));
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

    private static MenuCategory MakeMenuCategory(
        string name,
        int restaurantId,
        bool isActive = true) =>
            new MenuCategory(0, restaurantId, name, isActive);

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