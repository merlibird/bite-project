using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Implementation;
using NSubstitute;

namespace Bite.Tests.UnitTests;

public class MenuServiceTests
{
    private const int RestaurantId = 1;

    private readonly IRestaurantDao restaurantDao = Substitute.For<IRestaurantDao>();
    private readonly IMenuCategoryDao menuCategoryDao = Substitute.For<IMenuCategoryDao>();
    private readonly IMenuItemDao menuItemDao = Substitute.For<IMenuItemDao>();

    public MenuServiceTests()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new Restaurant(RestaurantId, "Test", 1, "https://hook", "key"));
        // Empty re-read by default; the GetMenuAsync tests override these explicitly.
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MenuCategory>());
        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MenuItem>());
    }

    private MenuService CreateService() => new(restaurantDao, menuCategoryDao, menuItemDao);

    private static Menu MenuWith(string categoryName, params MenuItem[] items)
        => new(RestaurantId, [new MenuCategoryWithItems(0, categoryName, items)]);

    private static MenuItem Item(string name, decimal price = 5m)
        => new(0, RestaurantId, name, null, price, isActive: true);

    // =====================================================================
    // GetMenuAsync
    // =====================================================================

    [Fact]
    public async Task GetMenuAsync_RestaurantNotFound_ReturnsNull()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var menu = await CreateService().GetMenuAsync(RestaurantId);

        Assert.Null(menu);
    }

    [Fact]
    public async Task GetMenuAsync_RestaurantExists_ReturnsMenuForRestaurant()
    {
        var menu = await CreateService().GetMenuAsync(RestaurantId);

        Assert.NotNull(menu);
        Assert.Equal(RestaurantId, menu!.RestaurantId);
    }

    [Fact]
    public async Task GetMenuAsync_AssignsItemsToTheirCategory()
    {
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new MenuCategory(10, RestaurantId, "Pizza"),
                new MenuCategory(20, RestaurantId, "Drinks"),
            });
        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new MenuItem(1, RestaurantId, "Margherita", null, 9m, true, menuCategoryIds: [10]),
                new MenuItem(2, RestaurantId, "Cola", null, 3m, true, menuCategoryIds: [20]),
            });

        var menu = await CreateService().GetMenuAsync(RestaurantId);

        var pizza = menu!.Categories.Single(c => c.Id == 10);
        var drinks = menu.Categories.Single(c => c.Id == 20);
        Assert.Equal("Margherita", pizza.Items.Single().Name);
        Assert.Equal("Cola", drinks.Items.Single().Name);
    }

    // =====================================================================
    // UpdateMenuAsync
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_RestaurantNotFound_ReturnsNotFound()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var result = await CreateService().UpdateMenuAsync(RestaurantId, MenuWith("Drinks", Item("Cola")));

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateMenuAsync_BlankCategoryName_ReturnsError(string categoryName)
    {
        var result = await CreateService().UpdateMenuAsync(RestaurantId, MenuWith(categoryName, Item("Cola")));

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateMenuAsync_BlankItemName_ReturnsError(string itemName)
    {
        var result = await CreateService().UpdateMenuAsync(RestaurantId, MenuWith("Drinks", Item(itemName)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_NegativeItemPrice_ReturnsError()
    {
        var result = await CreateService().UpdateMenuAsync(RestaurantId, MenuWith("Drinks", Item("Cola", price: -1m)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_ValidMenu_ReturnsSuccess()
    {
        var result = await CreateService().UpdateMenuAsync(RestaurantId, MenuWith("Drinks", Item("Cola")));

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceResultType.Success, result.ResultType);
    }
}
