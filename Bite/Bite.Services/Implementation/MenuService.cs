using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using System.Transactions;

namespace Bite.Services.Implementation;

public class MenuService(
    IRestaurantDao restaurantDao,
    IMenuCategoryDao menuCategoryDao,
    IMenuItemDao menuItemDao) : IMenuService
{
    public async Task<Menu?> GetMenuAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return null;
        }

        var categories = await menuCategoryDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken);
        var items = await menuItemDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken);

        var categoryWithItems = categories
            .Select(category => new MenuCategoryWithItems(
                id: category.Id,
                name: category.Name,
                items: items
                    .Where(item => item.MenuCategoryIds.Contains(category.Id))
                    .ToList()))
            .ToList();

        return new Menu(
            restaurantId: restaurantId,
            categories: categoryWithItems);
    }

    public async Task<ServiceResult<Menu>> UpdateMenuAsync(
        int restaurantId,
        Menu menu,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return ServiceResult<Menu>.Failure("Restaurant not found.", ServiceResultType.NotFound);
        }

        var validationError = ValidateMenu(menu);
        if (validationError is not null)
        {
            return ServiceResult<Menu>.Failure(validationError);
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await menuItemDao.DeactivateAndClearMenuAsync(restaurantId, cancellationToken);
            await menuCategoryDao.DeleteAllByRestaurantIdAsync(restaurantId, cancellationToken);

            foreach (var category in menu.Categories)
            {
                int categoryId = await menuCategoryDao.InsertAsync(
                    new MenuCategory(
                        id: 0,
                        restaurantId: restaurantId,
                        name: category.Name.Trim()),
                    cancellationToken);

                foreach (var item in category.Items)
                {
                    await menuItemDao.InsertAsync(
                        new MenuItem(
                            id: 0,
                            restaurantId: restaurantId,
                            name: item.Name.Trim(),
                            description: string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                            price: item.Price,
                            isActive: item.IsActive,
                            menuCategoryIds: [categoryId]),
                        cancellationToken);
                }
            }

            scope.Complete();
        }

        var updatedMenu = await GetMenuAsync(restaurantId, cancellationToken);
        return ServiceResult<Menu>.Success(updatedMenu!);
    }

    private static string? ValidateMenu(Menu menu)
    {
        foreach (var category in menu.Categories)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return "Category name is required.";
            }

            foreach (var item in category.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    return "Menu item name is required.";
                }

                if (item.Price < 0)
                {
                    return "Menu item price must not be negative.";
                }
            }
        }

        return null;
    }
}
