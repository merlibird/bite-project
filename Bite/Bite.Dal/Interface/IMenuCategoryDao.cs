using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IMenuCategoryDao
{
    Task<IEnumerable<MenuCategory>> FindAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<MenuCategory?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(MenuCategory menuCategory, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(MenuCategory menuCategory, CancellationToken cancellationToken = default);

    Task<int> DeleteAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
