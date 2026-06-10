using System;

namespace Bite.Domain
{
    public enum OrderStatus
    {
        Received,
        SentToRestaurant,
        InPreparation,
        OutForDelivery,
        Delivered
    }

    public static class OrderStatusExtensions
    {
        // Maps the enum to the exact codes stored in the database
        public static string ToDbValue(this OrderStatus status) => status switch
        {
            OrderStatus.Received => "RECEIVED",
            OrderStatus.SentToRestaurant => "SENT_TO_RESTAURANT",
            OrderStatus.InPreparation => "IN_PREPARATION",
            OrderStatus.OutForDelivery => "OUT_FOR_DELIVERY",
            OrderStatus.Delivered => "DELIVERED",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown order status.")
        };

        public static OrderStatus FromDbValue(string value) => value switch
        {
            "RECEIVED" => OrderStatus.Received,
            "SENT_TO_RESTAURANT" => OrderStatus.SentToRestaurant,
            "IN_PREPARATION" => OrderStatus.InPreparation,
            "OUT_FOR_DELIVERY" => OrderStatus.OutForDelivery,
            "DELIVERED" => OrderStatus.Delivered,
            _ => throw new ArgumentException($"Unknown order status code: '{value}'.", nameof(value))
        };
    }
}
