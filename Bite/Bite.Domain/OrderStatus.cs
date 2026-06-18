using System;

namespace Bite.Domain;

public enum OrderStatus
{
    Received,
    SentToRestaurant,
    InPreparation,
    OutForDelivery,
    Delivered,
    Cancelled
}

public static class OrderStatusExtensions
{
    public static string ToDbValue(this OrderStatus status) => status switch
    {
        OrderStatus.Received => "RECEIVED",
        OrderStatus.SentToRestaurant => "SENT_TO_RESTAURANT",
        OrderStatus.InPreparation => "IN_PREPARATION",
        OrderStatus.OutForDelivery => "OUT_FOR_DELIVERY",
        OrderStatus.Delivered => "DELIVERED",
        OrderStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown order status.")
    };

    public static OrderStatus FromDbValue(string value) => value switch
    {
        "RECEIVED" => OrderStatus.Received,
        "SENT_TO_RESTAURANT" => OrderStatus.SentToRestaurant,
        "IN_PREPARATION" => OrderStatus.InPreparation,
        "OUT_FOR_DELIVERY" => OrderStatus.OutForDelivery,
        "DELIVERED" => OrderStatus.Delivered,
        "CANCELLED" => OrderStatus.Cancelled,
        _ => throw new ArgumentException($"Unknown order status code: '{value}'.", nameof(value))
    };

    public static bool IsValidOrderStatus(string value) => value is
        "RECEIVED" or
        "SENT_TO_RESTAURANT" or
        "IN_PREPARATION" or
        "OUT_FOR_DELIVERY" or
        "DELIVERED" or
        "CANCELLED";

    public static bool CanTransitionTo(this OrderStatus current, OrderStatus next)
    {
        if (current == next) return true;
        if (current == OrderStatus.Cancelled || current == OrderStatus.Delivered) return false;

        return next switch
        {
            OrderStatus.SentToRestaurant => current == OrderStatus.Received,
            OrderStatus.InPreparation => current == OrderStatus.SentToRestaurant,
            OrderStatus.OutForDelivery => current == OrderStatus.InPreparation,
            OrderStatus.Delivered => current == OrderStatus.OutForDelivery,
            OrderStatus.Cancelled => current != OrderStatus.Delivered && current != OrderStatus.Cancelled,
            _ => false
        };
    }
}
