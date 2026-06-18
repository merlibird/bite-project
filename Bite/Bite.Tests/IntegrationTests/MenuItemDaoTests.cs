using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class MenuItemDaoTests : IAsyncLifetime
{
    private readonly MenuItemDao dao;
    private readonly MenuCategoryDao menuCategoryDao;
    private readonly RestaurantDao restaurantDao;
    private readonly AddressDao addressDao;
    private readonly AdoTemplate template;

    public MenuItemDaoTests(DatabaseFixture fixture)
    {
        dao = new MenuItemDao(fixture.ConnectionFactory);
        menuCategoryDao = new MenuCategoryDao(fixture.ConnectionFactory);
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
    public async Task FindAllByRestaurantIdAsync_TwoMenuItemsForRestaurant_ReturnsBothMenuItems()
    {
        int restaurantId = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));
        await dao.InsertAsync(MakeMenuItem("Salat", restaurantId));

        var result = await dao.FindAllByRestaurantIdAsync(restaurantId);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindAllByRestaurantIdAsync_OnlyReturnsItemsForGivenRestaurant()
    {
        int restaurantId1 = await SeedRestaurantAsync();
        int restaurantId2 = await SeedRestaurantAsync();

        await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId1));
        await dao.InsertAsync(MakeMenuItem("Pizza", restaurantId2));

        var result = await dao.FindAllByRestaurantIdAsync(restaurantId1);

        Assert.Single(result);
        Assert.Equal("Schnitzel", result.First().Name);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidMenuItem_ReturnsNewId()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoMenuItems_ReturnsDifferentIds()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id1 = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));
        var id2 = await dao.InsertAsync(MakeMenuItem("Salat", restaurantId));

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task InsertAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        int restaurantId = await SeedRestaurantAsync();
        string name = "Wiener Schnitzel";
        string description = "Klassiker mit Petersilkartoffeln";
        decimal price = 14.90m;
        bool isActive = true;

        var id = await dao.InsertAsync(MakeMenuItem(name, restaurantId, description, price, isActive));

        var result = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();

        Assert.Equal(name, result.Name);
        Assert.Equal(restaurantId, result.RestaurantId);
        Assert.Equal(description, result.Description);
        Assert.Equal(price, result.Price);
        Assert.Equal(isActive, result.IsActive);
    }

    [Fact]
    public async Task InsertAsync_NullableFieldNotSet_ReturnsNull()
    {
        int restaurantId = await SeedRestaurantAsync();
        await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId, description: null));

        var result = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();

        Assert.Null(result.Description);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingMenuItem_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));
        var menuItem = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();

        var result = await dao.UpdateAsync(menuItem);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsFalse()
    {
        int restaurantId = await SeedRestaurantAsync();
        // not using MakeMenuItem() to avoid inserting a new menu item with id=0
        var ghost = new MenuItem(69420, restaurantId, "Ghost Item", null, 9.99m, true);

        var result = await dao.UpdateAsync(ghost);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingMenuItem_PersistsChanges()
    {
        int restaurantId = await SeedRestaurantAsync();
        await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));
        var menuItem = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();
        menuItem.IsActive = false;

        await dao.UpdateAsync(menuItem);

        var updated = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();
        Assert.False(updated.IsActive);
    }

    // -------------------------------------------------------------------------
    // SetMenuCategoriesAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SetMenuCategoriesAsync_ValidCategoryIds_ReturnsTrue()
    {
        int restaurantId = await SeedRestaurantAsync();
        int categoryId = await SeedMenuCategoryAsync(restaurantId);
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));

        var result = await dao.SetMenuCategoriesAsync(id, [categoryId]);

        Assert.True(result);
    }

    [Fact]
    public async Task SetMenuCategoriesAsync_ValidCategoryIds_MenuItemHasCategories()
    {
        int restaurantId = await SeedRestaurantAsync();
        int categoryId1 = await SeedMenuCategoryAsync(restaurantId, "Kategorie 1");
        int categoryId2 = await SeedMenuCategoryAsync(restaurantId, "Kategorie 2");
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));

        await dao.SetMenuCategoriesAsync(id, [categoryId1, categoryId2]);

        var result = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();
        Assert.Contains(categoryId1, result.MenuCategoryIds);
        Assert.Contains(categoryId2, result.MenuCategoryIds);
    }

    [Fact]
    public async Task SetMenuCategoriesAsync_ReplacesExistingCategories()
    {
        int restaurantId = await SeedRestaurantAsync();
        int categoryId1 = await SeedMenuCategoryAsync(restaurantId, "Kategorie 1");
        int categoryId2 = await SeedMenuCategoryAsync(restaurantId, "Kategorie 2");
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));

        await dao.SetMenuCategoriesAsync(id, [categoryId1]);
        await dao.SetMenuCategoriesAsync(id, [categoryId2]);

        var result = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();
        Assert.DoesNotContain(categoryId1, result.MenuCategoryIds);
        Assert.Contains(categoryId2, result.MenuCategoryIds);
    }

    [Fact]
    public async Task SetMenuCategoriesAsync_EmptyList_ClearsCategories()
    {
        int restaurantId = await SeedRestaurantAsync();
        int categoryId = await SeedMenuCategoryAsync(restaurantId);
        var id = await dao.InsertAsync(MakeMenuItem("Schnitzel", restaurantId));
        await dao.SetMenuCategoriesAsync(id, [categoryId]);

        await dao.SetMenuCategoriesAsync(id, []);

        var result = (await dao.FindAllByRestaurantIdAsync(restaurantId)).Single();
        Assert.Empty(result.MenuCategoryIds);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static MenuItem MakeMenuItem(
        string name,
        int restaurantId,
        string? description = "Sehr lecker",
        decimal price = 9.99m,
        bool isActive = true) =>
            new MenuItem(0, restaurantId, name, description, price, isActive);

    private async Task<int> SeedAddressAsync() =>
        await addressDao.InsertAsync(
            new Address(0, "Hauptstraße", "1", "4040", "Linz", "Austria", 48.3, 14.2, null));

    private async Task<int> SeedRestaurantAsync()
    {
        int addressId = await SeedAddressAsync();
        return await restaurantDao.InsertAsync(
            new Restaurant(0, "Testrestaurant", addressId, "https://example.com/webhook", Guid.NewGuid().ToString()));
    }

    private async Task<int> SeedMenuCategoryAsync(int restaurantId, string name = "Testkategorie") =>
        await menuCategoryDao.InsertAsync(
            new MenuCategory(0, restaurantId, name));
}