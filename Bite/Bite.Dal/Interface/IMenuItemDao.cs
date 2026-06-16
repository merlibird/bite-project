using Bite.Domain;

using System.Threading;

namespace Bite.Dal.Interface;

public interface IMenuItemDao
{
    Task<MenuItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<MenuItem>> FindAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    Task<bool> SetMenuCategoriesAsync(int menuItemId, IEnumerable<int> menuCategoryIds, CancellationToken cancellationToken = default);

    Task<int> DeleteAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<bool> DeactivateAndClearMenuAsync(int restaurantId, CancellationToken cancellationToken = default);
}
