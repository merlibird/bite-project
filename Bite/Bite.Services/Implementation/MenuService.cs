using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;

namespace Bite.Services.Implementation;

public class MenuService : IMenuService
{
    private readonly IRestaurantDao restaurantDao;
    private readonly IMenuCategoryDao menuCategoryDao;
    private readonly IMenuItemDao menuItemDao;

    public MenuService(
        IRestaurantDao restaurantDao,
        IMenuCategoryDao menuCategoryDao,
        IMenuItemDao menuItemDao)
    {
        this.restaurantDao = restaurantDao;
        this.menuCategoryDao = menuCategoryDao;
        this.menuItemDao = menuItemDao;
    }

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
            categories: (IEnumerable<MenuCategory>)categoryWithItems);
    }
}