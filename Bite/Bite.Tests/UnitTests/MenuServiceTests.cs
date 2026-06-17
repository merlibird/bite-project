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
            .Returns(new Restaurant(RestaurantId, "Test Restaurant", 1, "https://hook", "key"));

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MenuCategory>());

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MenuItem>());
    }

    private MenuService CreateService()
        => new(restaurantDao, menuCategoryDao, menuItemDao);

    private static MenuCategory Category(
        int id,
        string name,
        bool isActive = true)
        => new(id, RestaurantId, name, isActive);

    private static MenuItem Item(
        string name,
        decimal price = 5m,
        int id = 0,
        bool isActive = true,
        string? description = null,
        int[]? categoryIds = null)
        => new(
            id,
            RestaurantId,
            name,
            description,
            price,
            isActive,
            menuCategoryIds: categoryIds ?? []);

    private static Menu MenuWith(params MenuCategoryWithItems[] categories)
        => new(RestaurantId, categories);

    private static MenuCategoryWithItems CategoryWithItems(
        int id,
        string name,
        bool isActive = true,
        params MenuItem[] items)
        => new(
            id: id,
            name: name,
            isActive: isActive,
            items: items);

    // =====================================================================
    // GetMenuAsync
    // =====================================================================

    [Fact]
    public async Task GetMenuAsync_RestaurantNotFound_ReturnsNotFound()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var result = await CreateService().GetMenuAsync(RestaurantId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task GetMenuAsync_RestaurantExists_ReturnsMenuForRestaurant()
    {
        var result = await CreateService().GetMenuAsync(RestaurantId);

        Assert.True(result.IsSuccess);
        var menu = result.Data!;
        Assert.Equal(RestaurantId, menu.RestaurantId);
        Assert.Empty(menu.Categories);
    }

    [Fact]
    public async Task GetMenuAsync_FiltersInactiveCategories()
    {
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Category(10, "Active Category", isActive: true),
                Category(20, "Inactive Category", isActive: false),
            });

        var menu = (await CreateService().GetMenuAsync(RestaurantId)).Data!;

        Assert.Single(menu.Categories);
        Assert.Equal("Active Category", menu.Categories.Single().Name);
    }

    [Fact]
    public async Task GetMenuAsync_FiltersInactiveItems()
    {
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Category(10, "Pizza"),
            });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Item("Margherita", id: 1, isActive: true, categoryIds: [10]),
                Item("Inactive Pizza", id: 2, isActive: false, categoryIds: [10]),
            });

        var menu = (await CreateService().GetMenuAsync(RestaurantId)).Data!;

        var category = menu.Categories.Single();

        Assert.Equal("Pizza", category.Name);
        Assert.Single(category.Items);
        Assert.Equal("Margherita", category.Items.Single().Name);
    }

    [Fact]
    public async Task GetMenuAsync_AssignsItemsToMatchingCategories()
    {
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Category(10, "Pizza"),
                Category(20, "Drinks"),
            });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Item("Margherita", id: 1, categoryIds: [10]),
                Item("Cola", id: 2, categoryIds: [20]),
            });

        var menu = (await CreateService().GetMenuAsync(RestaurantId)).Data!;

        var pizzaCategory = menu.Categories.Single(c => c.Id == 10);
        var drinksCategory = menu.Categories.Single(c => c.Id == 20);

        Assert.Single(pizzaCategory.Items);
        Assert.Equal("Margherita", pizzaCategory.Items.Single().Name);

        Assert.Single(drinksCategory.Items);
        Assert.Equal("Cola", drinksCategory.Items.Single().Name);
    }

    [Fact]
    public async Task GetMenuAsync_ItemCanAppearInMultipleCategories()
    {
        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Category(10, "Sides"),
                Category(20, "Popular"),
            });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                Item("Fries", id: 1, categoryIds: [10, 20]),
            });

        var menu = (await CreateService().GetMenuAsync(RestaurantId)).Data!;

        Assert.Contains(menu.Categories.Single(c => c.Id == 10).Items, i => i.Name == "Fries");
        Assert.Contains(menu.Categories.Single(c => c.Id == 20).Items, i => i.Name == "Fries");
    }

    // =====================================================================
    // UpdateMenuAsync - Restaurant lookup
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_RestaurantNotFound_ReturnsNotFound()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita")]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_RestaurantNotFound_DoesNotReadOrWriteMenuData()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita")]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.DidNotReceive()
            .FindAllByRestaurantIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .FindAllByRestaurantIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());

        await menuCategoryDao.DidNotReceive()
            .InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // UpdateMenuAsync - Validation
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_CategoryNameMissing_ReturnsValidationError()
    {
        var menu = MenuWith(
            CategoryWithItems(0, "   ", items: [Item("Margherita")]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_ItemNameMissing_ReturnsValidationError()
    {
        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("   ")]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_ItemPriceNegative_ReturnsValidationError()
    {
        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita", price: -1m)]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task UpdateMenuAsync_ValidationError_DoesNotWriteAnything()
    {
        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita", price: -1m)]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.DidNotReceive()
            .InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>());

        await menuCategoryDao.DidNotReceive()
            .UpdateAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .UpdateAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .SetMenuCategoriesAsync(Arg.Any<int>(), Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // UpdateMenuAsync - Category creation and update
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_NewCategory_InsertsCategory()
    {
        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        var menu = MenuWith(
            CategoryWithItems(0, "Pizza"));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);

        await menuCategoryDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuCategory>(c =>
                    c.Id == 0 &&
                    c.RestaurantId == RestaurantId &&
                    c.Name == "Pizza" &&
                    c.IsActive),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_NewCategory_TrimsNameBeforeInsert()
    {
        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        var menu = MenuWith(
            CategoryWithItems(0, "  Pizza  "));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuCategory>(c => c.Name == "Pizza"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingCategoryById_UpdatesCategory()
    {
        var existingCategory = Category(10, "Old Pizza", isActive: true);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        var menu = MenuWith(
            CategoryWithItems(10, "New Pizza", isActive: true));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);

        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c =>
                    c.Id == 10 &&
                    c.Name == "New Pizza" &&
                    c.IsActive),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingCategoryByNameIgnoringCase_UpdatesInsteadOfInserting()
    {
        var existingCategory = Category(10, "Pizza", isActive: true);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        var menu = MenuWith(
            CategoryWithItems(0, "  pizza  ", isActive: true));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 10 && c.Name == "pizza"),
                Arg.Any<CancellationToken>());

        await menuCategoryDao.DidNotReceive()
            .InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_CategoryInRequestWithIsActiveFalse_UpdatesCategoryToInactive()
    {
        var existingCategory = Category(10, "Pizza", isActive: true);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        var menu = MenuWith(
            CategoryWithItems(10, "Pizza", isActive: false));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 10 && !c.IsActive),
                Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // UpdateMenuAsync - Item creation and update
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_NewItem_InsertsItem()
    {
        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(100);

        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita", price: 9.90m)]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);

        await menuItemDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuItem>(i =>
                    i.Id == 0 &&
                    i.RestaurantId == RestaurantId &&
                    i.Name == "Margherita" &&
                    i.Price == 9.90m &&
                    i.IsActive &&
                    i.MenuCategoryIds.Contains(10)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_NewItem_TrimsNameAndDescriptionBeforeInsert()
    {
        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(100);

        var menu = MenuWith(
            CategoryWithItems(
                0,
                "Pizza",
                items:
                [
                    Item(
                        name: "  Margherita  ",
                        description: "  Tomato and cheese  ")
                ]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuItem>(i =>
                    i.Name == "Margherita" &&
                    i.Description == "Tomato and cheese"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_NewItem_WhitespaceDescriptionIsStoredAsNull()
    {
        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(100);

        var menu = MenuWith(
            CategoryWithItems(
                0,
                "Pizza",
                items: [Item("Margherita", description: "   ")]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuItem>(i => i.Description == null),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingItemById_UpdatesItem()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Old Name", id: 100, price: 8m, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items: [Item("New Name", price: 11.50m, id: 100, description: "Updated")]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);

        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i =>
                    i.Id == 100 &&
                    i.RestaurantId == RestaurantId &&
                    i.Name == "New Name" &&
                    i.Description == "Updated" &&
                    i.Price == 11.50m &&
                    i.IsActive),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingItemById_UpdatesMenuCategoryAssignment()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Margherita", id: 100, categoryIds: [999]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items: [Item("Margherita", id: 100)]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .SetMenuCategoriesAsync(
                100,
                Arg.Is<IReadOnlyCollection<int>>(ids =>
                    ids.Count == 1 &&
                    ids.Contains(10)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingItemById_CanBeSetInactive()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Margherita", id: 100, isActive: true, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items: [Item("Margherita", id: 100, isActive: false)]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 100 && !i.IsActive),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ItemWithIdZero_IsInsertedAsFreshRecordEvenWhenSimilarItemExists()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Margherita", id: 100, isActive: true, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(200);

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items: [Item("Margherita", id: 0)]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuItem>(i => i.Id == 0 && i.Name == "Margherita"),
                Arg.Any<CancellationToken>());

        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 100 && !i.IsActive),
                Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // UpdateMenuAsync - Soft delete missing categories and items
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_CategoryMissingFromRequest_DeactivatesCategory()
    {
        var existingCategory = Category(10, "Old Category", isActive: true);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        var menu = MenuWith(
            CategoryWithItems(0, "New Category"));

        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(20);

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 10 && !c.IsActive),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_InactiveCategoryMissingFromRequest_IsNotUpdatedAgain()
    {
        var existingCategory = Category(10, "Already Inactive", isActive: false);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        var menu = MenuWith(
            CategoryWithItems(0, "New Category"));

        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(20);

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuCategoryDao.DidNotReceive()
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 10),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ItemMissingFromRequest_DeactivatesItemAndClearsCategoryAssignments()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Old Pizza", id: 100, isActive: true, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(10, "Pizza"));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 100 && !i.IsActive),
                Arg.Any<CancellationToken>());

        await menuItemDao.Received(1)
            .SetMenuCategoriesAsync(
                100,
                Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 0),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_InactiveItemMissingFromRequest_IsNotUpdatedAgain()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Old Pizza", id: 100, isActive: false, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(10, "Pizza"));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.DidNotReceive()
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 100),
                Arg.Any<CancellationToken>());

        await menuItemDao.DidNotReceive()
            .SetMenuCategoriesAsync(
                100,
                Arg.Any<IReadOnlyCollection<int>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ExistingItemPresentInRequest_IsNotSoftDeleted()
    {
        var existingCategory = Category(10, "Pizza");
        var existingItem = Item("Margherita", id: 100, isActive: true, categoryIds: [10]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingItem });

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items: [Item("Margherita", id: 100)]));

        await CreateService().UpdateMenuAsync(RestaurantId, menu);

        await menuItemDao.DidNotReceive()
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 100 && !i.IsActive),
                Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // UpdateMenuAsync - Combined scenarios
    // =====================================================================

    [Fact]
    public async Task UpdateMenuAsync_MixedRequest_UpdatesExistingCreatesNewAndSoftDeletesMissing()
    {
        var existingPizzaCategory = Category(10, "Old Pizza", isActive: true);
        var existingDrinksCategory = Category(20, "Drinks", isActive: true);

        var existingPizza = Item("Old Margherita", id: 100, isActive: true, categoryIds: [10]);
        var existingCola = Item("Cola", id: 200, isActive: true, categoryIds: [20]);

        menuCategoryDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingPizzaCategory, existingDrinksCategory });

        menuItemDao.FindAllByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingPizza, existingCola });

        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(30);

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(300);

        var menu = MenuWith(
            CategoryWithItems(
                10,
                "Pizza",
                items:
                [
                    Item("Margherita", id: 100, price: 9.90m),
                    Item("Salami", id: 0, price: 11.50m)
                ]),
            CategoryWithItems(
                0,
                "Desserts",
                items:
                [
                    Item("Tiramisu", id: 0, price: 6.50m)
                ]));

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceResultType.Success, result.ResultType);

        // Existing category was updated.
        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 10 && c.Name == "Pizza" && c.IsActive),
                Arg.Any<CancellationToken>());

        // Missing category was deactivated.
        await menuCategoryDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuCategory>(c => c.Id == 20 && !c.IsActive),
                Arg.Any<CancellationToken>());

        // New category was inserted.
        await menuCategoryDao.Received(1)
            .InsertAsync(
                Arg.Is<MenuCategory>(c => c.Name == "Desserts" && c.IsActive),
                Arg.Any<CancellationToken>());

        // Existing item was updated.
        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i =>
                    i.Id == 100 &&
                    i.Name == "Margherita" &&
                    i.Price == 9.90m &&
                    i.IsActive),
                Arg.Any<CancellationToken>());

        // Missing item was deactivated.
        await menuItemDao.Received(1)
            .UpdateAsync(
                Arg.Is<MenuItem>(i => i.Id == 200 && !i.IsActive),
                Arg.Any<CancellationToken>());

        // New items were inserted.
        await menuItemDao.Received(2)
            .InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>());

        await menuItemDao.Received(1)
            .SetMenuCategoriesAsync(
                200,
                Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 0),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMenuAsync_ValidMenu_ReturnsSuccessWithMenu()
    {
        var menu = MenuWith(
            CategoryWithItems(0, "Pizza", items: [Item("Margherita")]));

        menuCategoryDao.InsertAsync(Arg.Any<MenuCategory>(), Arg.Any<CancellationToken>())
            .Returns(10);

        menuItemDao.InsertAsync(Arg.Any<MenuItem>(), Arg.Any<CancellationToken>())
            .Returns(100);

        var result = await CreateService().UpdateMenuAsync(RestaurantId, menu);

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceResultType.Success, result.ResultType);
        Assert.NotNull(result.Data);
        Assert.Equal(RestaurantId, result.Data!.RestaurantId);
    }
}
