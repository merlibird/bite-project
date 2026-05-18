using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class DeliveryZone(
        int id,
        int restaurantId,
        decimal minOrderValue,
        double maxDistance)
    {
        public int Id { get; init; } = id;
        public int RestaurantId { get; init; } = restaurantId;
        public decimal MinOrderValue { get; set; } = minOrderValue;
        public double MaxDistance { get; set; } = maxDistance;  // in km

        public override string ToString()
        {
            return $"DeliveryZone {Id}: RestaurantId: {RestaurantId}, MinOrderValue: {MinOrderValue}, MaxDistance: {MaxDistance} km";
        }
    }
}
