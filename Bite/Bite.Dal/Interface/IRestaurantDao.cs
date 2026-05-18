using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IRestaurantDao
{
    Task<IEnumerable<Restaurant>> FindAllAsync();

    Task<Restaurant?> FindByIdAsync(int id);

    Task<int?> InsertAsync(Restaurant restaurant);

    Task<bool> UpdateAsync(Restaurant restaurant);

    Task<bool> DeleteAsync(int id);
}
