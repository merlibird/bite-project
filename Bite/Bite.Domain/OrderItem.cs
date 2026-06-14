using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class OrderItem(
        int id,
        int orderId,
        int menuItemId,
        int quantity,
        decimal unitPrice)
    {
        public int Id { get; init; } = id;
        public int OrderId { get; init; } = orderId;
        public int MenuItemId { get; init; } = menuItemId;
        public int Quantity { get; set; } = quantity;
        public decimal UnitPrice { get; init; } = unitPrice;

        public override string ToString() =>
            $"OrderItem {Id}: OrderId: {OrderId}, MenuItemId: {MenuItemId}, Quantity: {Quantity}, UnitPrice: {UnitPrice}";
    }
}
