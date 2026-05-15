using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IDeliveryFeeRuleDao
{
    Task<IEnumerable<DeliveryFeeRule>> FindByRestaurantIdAsync(int restaurantId);

    Task<IEnumerable<DeliveryFeeRule>> FindByRestaurantIdAndZoneIdAsync(int restaurantId, int zoneId);

    Task<int?> InsertAsync(DeliveryFeeRule deliveryFeeRule);

    Task<bool> UpdateAsync(DeliveryFeeRule deliveryFeeRule);

    Task<bool> DeleteAsync(int id);
}
