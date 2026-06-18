using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IOpeningHourSlotDao
{
    Task<IEnumerable<OpeningHourSlot>> FindByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<IEnumerable<OpeningHourSlot>> FindAllAsync(CancellationToken cancellationToken = default);

    Task<int> InsertAsync(OpeningHourSlot openingHourSlot, CancellationToken cancellationToken = default);
}
