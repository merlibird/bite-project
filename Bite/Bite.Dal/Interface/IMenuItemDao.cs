using Bite.Domain;

namespace Bite.Dal.Interface;

public interface IMenuItemDao
{
    Task<IEnumerable<MenuItem>> FindAllAsync();

    Task<IEnumerable<MenuItem>> FindAllByRestaurantIdAsync(int restaurantId);

    Task<int?> InsertAsync(MenuItem menuItem);

    Task<bool> UpdateAsync(MenuItem menuItem);
}