using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class CustomerOrder(
        int id,
        int restaurantId,
        int addressId,
        string orderCode,
        OrderStatus status,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        public int Id { get; init; } = id;
        public int RestaurantId { get; init; } = restaurantId;
        public int AddressId { get; init; } = addressId;
        public string OrderCode { get; init; } = orderCode;
        public OrderStatus Status { get; set; } = status;
        public DateTime? CreatedAt { get; init; } = createdAt;
        public DateTime? UpdatedAt { get; init; } = updatedAt;

        public override string ToString() =>
            $"CustomerOrder {Id}: {OrderCode} (RestaurantId: {RestaurantId}, AddressId: {AddressId}), " +
            $"Status: {Status}, CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}";
    }
}
