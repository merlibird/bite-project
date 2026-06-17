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
    public async Task<ServiceResult<Menu>> GetMenuAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return ServiceResult<Menu>.Failure("Restaurant not found.", ServiceResultType.NotFound);
        }

        var categories = (await menuCategoryDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken))
            .Where(c => c.IsActive);
        var items = (await menuItemDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken))
            .Where(i => i.IsActive);

        var categoryWithItems = categories
            .Select(category => new MenuCategoryWithItems(
                id: category.Id,
                name: category.Name,
                isActive: category.IsActive,
                items: items
                    .Where(item => item.MenuCategoryIds.Contains(category.Id))
                    .ToList()))
            .ToList();

        return ServiceResult<Menu>.Success(new Menu(
            restaurantId: restaurantId,
            categories: categoryWithItems));
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
            return ServiceResult<Menu>.Failure(validationError, ServiceResultType.ValidationError);
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var existingCategories = await menuCategoryDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken);
            var existingItems = await menuItemDao.FindAllByRestaurantIdAsync(restaurantId, cancellationToken);

            var processedCategoryIds = new List<int>();
            var processedItemIds = new List<int>();

            // 1. Process Categories and their items in one pass to avoid ID collision issues
            foreach (var category in menu.Categories)
            {
                var existingCategory = existingCategories.FirstOrDefault(c => c.Id == category.Id && c.Id != 0);
                
                if (existingCategory == null)
                {
                    existingCategory = existingCategories.FirstOrDefault(c => c.Name.Equals(category.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                int dbCategoryId;
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name.Trim();
                    existingCategory.IsActive = category.IsActive;
                    await menuCategoryDao.UpdateAsync(existingCategory, cancellationToken);
                    dbCategoryId = existingCategory.Id;
                }
                else
                {
                    dbCategoryId = await menuCategoryDao.InsertAsync(
                        new MenuCategory(0, restaurantId, category.Name.Trim(), category.IsActive),
                        cancellationToken);
                }
                processedCategoryIds.Add(dbCategoryId);

                // 2. Process Items within this specific category
                foreach (var item in category.Items)
                {
                    var existingItem = existingItems.FirstOrDefault(i => i.Id == item.Id && i.Id != 0);
                    
                    if (existingItem != null)
                    {
                        var updatedItem = new MenuItem(
                            existingItem.Id,
                            restaurantId,
                            item.Name.Trim(),
                            string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                            item.Price,
                            item.IsActive,
                            menuCategoryIds: [dbCategoryId]);
                        
                        await menuItemDao.UpdateAsync(updatedItem, cancellationToken);
                        await menuItemDao.SetMenuCategoriesAsync(updatedItem.Id, [dbCategoryId], cancellationToken);
                        processedItemIds.Add(updatedItem.Id);
                    }
                    else
                    {
                        var newItem = new MenuItem(
                            0,
                            restaurantId,
                            item.Name.Trim(),
                            string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                            item.Price,
                            item.IsActive,
                            menuCategoryIds: [dbCategoryId]);
                        
                        int newItemId = await menuItemDao.InsertAsync(newItem, cancellationToken);
                        processedItemIds.Add(newItemId);
                    }
                }
            }

            // Deactivate categories not present in the request
            foreach (var existing in existingCategories.Where(c => c.IsActive && !processedCategoryIds.Contains(c.Id)))
            {
                existing.IsActive = false;
                await menuCategoryDao.UpdateAsync(existing, cancellationToken);
            }

            foreach (var existing in existingItems.Where(i => i.IsActive && !processedItemIds.Contains(i.Id)))
            {
                existing.IsActive = false;
                await menuItemDao.UpdateAsync(existing, cancellationToken);
                await menuItemDao.SetMenuCategoriesAsync(existing.Id, [], cancellationToken);
            }

            scope.Complete();
        }

        var updatedMenu = await GetMenuAsync(restaurantId, cancellationToken);
        return ServiceResult<Menu>.Success(updatedMenu.Data!);
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
