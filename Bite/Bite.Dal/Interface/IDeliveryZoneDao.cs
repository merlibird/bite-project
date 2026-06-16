using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IDeliveryZoneDao
{
    Task<IEnumerable<DeliveryZone>> FindByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(DeliveryZone deliveryZone, CancellationToken cancellationToken = default);

    Task<int> DeleteAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);
}
