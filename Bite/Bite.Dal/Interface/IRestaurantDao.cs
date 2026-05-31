using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IRestaurantDao
{
    Task<IEnumerable<Restaurant>> FindAllAsync(CancellationToken cancellationToken = default);

    Task<Restaurant?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Restaurant?> FindByApiKeyAsync(string hashedApiKey, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(Restaurant restaurant, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
