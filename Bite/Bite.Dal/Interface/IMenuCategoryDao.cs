using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IMenuCategoryDao
{
    Task<IEnumerable<MenuCategory>> FindAllAsync();

    Task<IEnumerable<MenuCategory>> FindAllByRestaurantIdAsync(int restaurantId);

    Task<MenuCategory?> FindByIdAsync(int id);

    Task<int?> InsertAsync(MenuCategory menuCategory);

    Task<bool> UpdateAsync(MenuCategory menuCategory);

    Task<bool> DeleteAsync(int id);
}
