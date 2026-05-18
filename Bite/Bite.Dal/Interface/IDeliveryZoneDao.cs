using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Interface;

public interface IDeliveryZoneDao
{
    Task<IEnumerable<DeliveryZone>> FindByRestaurantIdAsync(int restaurantId);

    Task<int?> InsertAsync(DeliveryZone deliveryZone);

    Task<bool> UpdateAsync(DeliveryZone deliveryZone);

    Task<bool> DeleteAsync(int id);
}
