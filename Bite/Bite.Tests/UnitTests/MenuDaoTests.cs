using Bite.Dal.Ado;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.UnitTests;

[Collection("Database")]
public class MenuDaoTests : IClassFixture<DatabaseFixture>
{
    private readonly MenuDao menuDao;
    private readonly MenuItemDao menuItemDao;
    private readonly MenuCategoryDao categoryDao;
    private readonly string connectionString;

    public MenuDaoTests(DatabaseFixture fixture)
    {
        var config = fixture.DbConfig;
        menuDao = new MenuDao(config);
        menuItemDao = new MenuItemDao(config);
        categoryDao = new MenuCategoryDao(config);
        connectionString = fixture.ConnectionString;
    }

    // -----------------------------------------------------------------------
    // InsertAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ShouldReturnPositiveId()
    {
        int menuId = await menuDao.InsertAsync();

        Assert.True(menuId > 0);

        await CleanupMenuAsync(menuId);
    }

    [Fact]
    public async Task InsertAsync_TwoCalls_ShouldReturnDifferentIds()
    {
        int id1 = await menuDao.InsertAsync();
        int id2 = await menuDao.InsertAsync();

        Assert.NotEqual(id1, id2);

        await CleanupMenuAsync(id1);
        await CleanupMenuAsync(id2);
    }

    // -----------------------------------------------------------------------
    // FindByRestaurantIdAsync
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public async Task FindByRestaurantIdAsync_ShouldReturnMenu(string restaurantName)
    {
        int restaurantId = await GetRestaurantIdByNameAsync(restaurantName);

        var menu = await menuDao.FindByRestaurantIdAsync(restaurantId);

        Assert.NotNull(menu);
        Assert.True(menu.Id > 0);
    }

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public async Task FindByRestaurantIdAsync_ShouldIncludeMenuItems(string restaurantName)
    {
        int restaurantId = await GetRestaurantIdByNameAsync(restaurantName);

        var menu = await menuDao.FindByRestaurantIdAsync(restaurantId);

        Assert.NotNull(menu);
        Assert.NotEmpty(menu.Items);
    }

    [Fact]
    public async Task FindByRestaurantIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var menu = await menuDao.FindByRestaurantIdAsync(-1);

        Assert.Null(menu);
    }

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public async Task FindByRestaurantIdAsync_AllItemsShouldHavePositivePrice(string restaurantName)
    {
        int restaurantId = await GetRestaurantIdByNameAsync(restaurantName);

        var menu = await menuDao.FindByRestaurantIdAsync(restaurantId);

        Assert.NotNull(menu);
        Assert.All(menu.Items, item => Assert.True(item.Price > 0));
    }

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public async Task FindByRestaurantIdAsync_AllItemsShouldHaveValidMenuId(string restaurantName)
    {
        int restaurantId = await GetRestaurantIdByNameAsync(restaurantName);

        var menu = await menuDao.FindByRestaurantIdAsync(restaurantId);

        Assert.NotNull(menu);
        Assert.All(menu.Items, item => Assert.Equal(menu.Id, item.MenuId));
    }

    [Theory]
    [InlineData("Restaurant Nimmersatt", "Margherita")]
    [InlineData("Restaurant Nimmersatt", "Lasagne al Forno")]
    [InlineData("Burger Bude Wien", "Classic Burger")]
    [InlineData("Sakura Sushi", "California Roll (8 St.)")]
    public async Task FindByRestaurantIdAsync_ShouldContainSpecificItem(
        string restaurantName, string expectedItem)
    {
        int restaurantId = await GetRestaurantIdByNameAsync(restaurantName);

        var menu = await menuDao.FindByRestaurantIdAsync(restaurantId);

        Assert.NotNull(menu);
        Assert.Contains(menu.Items, i => i.Name == expectedItem);
    }

    // -----------------------------------------------------------------------
    // ReplaceMenuItemsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ReplaceMenuItemsAsync_ShouldReplaceAllItems()
    {
        int menuId = await menuDao.InsertAsync();
        int catId = await categoryDao.InsertAsync("TestKategorie");

        // Initiale Items einfügen
        await menuItemDao.InsertAsync(new MenuItem { MenuId = menuId, CategoryId = catId, Name = "Alt 1", Price = 5.00m });
        await menuItemDao.InsertAsync(new MenuItem { MenuId = menuId, CategoryId = catId, Name = "Alt 2", Price = 6.00m });

        // Ersetzen mit neuen Items
        var newItems = new List<MenuItem>
        {
            new() { MenuId = menuId, CategoryId = catId, Name = "Neu 1", Price = 9.00m },
            new() { MenuId = menuId, CategoryId = catId, Name = "Neu 2", Price = 10.00m },
            new() { MenuId = menuId, CategoryId = catId, Name = "Neu 3", Price = 11.00m },
        };

        bool success = await menuDao.ReplaceMenuItemsAsync(menuId, newItems);

        var loaded = await menuItemDao.FindByMenuIdAsync(menuId);

        Assert.True(success);
        Assert.Equal(3, loaded.Count());
        Assert.All(loaded, item => Assert.StartsWith("Neu", item.Name));

        await CleanupMenuAsync(menuId);
    }

    [Fact]
    public async Task ReplaceMenuItemsAsync_OldItemsShouldNoLongerExist()
    {
        int menuId = await menuDao.InsertAsync();
        int catId = await categoryDao.InsertAsync("TestKategorie");

        await menuItemDao.InsertAsync(new MenuItem { MenuId = menuId, CategoryId = catId, Name = "Wird gelöscht", Price = 5.00m });

        await menuDao.ReplaceMenuItemsAsync(menuId, new List<MenuItem>
        {
            new() { MenuId = menuId, CategoryId = catId, Name = "Neues Item", Price = 8.00m }
        });

        var items = await menuItemDao.FindByMenuIdAsync(menuId);

        Assert.DoesNotContain(items, i => i.Name == "Wird gelöscht");

        await CleanupMenuAsync(menuId);
    }

    [Fact]
    public async Task ReplaceMenuItemsAsync_WithEmptyList_ShouldClearAllItems()
    {
        int menuId = await menuDao.InsertAsync();
        int catId = await categoryDao.InsertAsync("TestKategorie");

        await menuItemDao.InsertAsync(new MenuItem { MenuId = menuId, CategoryId = catId, Name = "Zu löschen", Price = 5.00m });

        await menuDao.ReplaceMenuItemsAsync(menuId, new List<MenuItem>());

        var items = await menuItemDao.FindByMenuIdAsync(menuId);

        Assert.Empty(items);

        await CleanupMenuAsync(menuId);
    }

    [Fact]
    public async Task ReplaceMenuItemsAsync_ShouldPersistPricesCorrectly()
    {
        int menuId = await menuDao.InsertAsync();
        int catId = await categoryDao.InsertAsync("TestKategorie");

        var newItems = new List<MenuItem>
        {
            new() { MenuId = menuId, CategoryId = catId, Name = "Preis Test", Price = 12.99m }
        };

        await menuDao.ReplaceMenuItemsAsync(menuId, newItems);

        var loaded = await menuItemDao.FindByMenuIdAsync(menuId);

        Assert.Equal(12.99m, loaded.Single().Price);

        await CleanupMenuAsync(menuId);
    }

    [Fact]
    public async Task ReplaceMenuItemsAsync_ShouldBeAtomic_OnError_ShouldNotPartiallyUpdate()
    {
        int menuId = await menuDao.InsertAsync();
        int catId = await categoryDao.InsertAsync("TestKategorie");

        // Originales Item einfügen
        await menuItemDao.InsertAsync(new MenuItem
        { MenuId = menuId, CategoryId = catId, Name = "Original", Price = 5.00m });

        // Ungültige category_id -1 provoziert FK-Fehler → Rollback
        var badItems = new List<MenuItem>
        {
            new() { MenuId = menuId, CategoryId = catId,  Name = "Gültig",   Price = 8.00m },
            new() { MenuId = menuId, CategoryId = -1,     Name = "Ungültig", Price = 9.00m },
        };

        await Assert.ThrowsAnyAsync<Exception>(
            () => menuDao.ReplaceMenuItemsAsync(menuId, badItems));

        // Nach Rollback muss das Original noch da sein
        var items = await menuItemDao.FindByMenuIdAsync(menuId);
        Assert.Contains(items, i => i.Name == "Original");

        await CleanupMenuAsync(menuId);
    }

    // -----------------------------------------------------------------------
    // Hilfsmethoden
    // -----------------------------------------------------------------------

    private async Task<int> GetRestaurantIdByNameAsync(string name)
    {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = new Microsoft.Data.SqlClient.SqlCommand(
            "SELECT id FROM Restaurant WHERE name = @name", connection);
        cmd.Parameters.AddWithValue("@name", name);

        var result = await cmd.ExecuteScalarAsync();
        if (result is null)
            throw new InvalidOperationException($"Restaurant '{name}' nicht in DB gefunden.");

        return Convert.ToInt32(result);
    }

    private async Task CleanupMenuAsync(int menuId)
    {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var deleteItems = new Microsoft.Data.SqlClient.SqlCommand(
            "DELETE FROM MenuItem WHERE menu_id = @id", connection);
        deleteItems.Parameters.AddWithValue("@id", menuId);
        await deleteItems.ExecuteNonQueryAsync();

        await using var deleteMenu = new Microsoft.Data.SqlClient.SqlCommand(
            "DELETE FROM Menu WHERE id = @id", connection);
        deleteMenu.Parameters.AddWithValue("@id", menuId);
        await deleteMenu.ExecuteNonQueryAsync();
    }
}

