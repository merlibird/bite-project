using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Bite.Dal.Interface;

public interface IDeliveryFeeRuleDao
{
    Task<IEnumerable<DeliveryFeeRule>> FindByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default);

    Task<IEnumerable<DeliveryFeeRule>> FindByRestaurantIdAndZoneIdAsync(int restaurantId, int zoneId, CancellationToken cancellationToken = default);

    Task<int?> InsertAsync(DeliveryFeeRule deliveryFeeRule, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(DeliveryFeeRule deliveryFeeRule, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
