using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class OrderStatusToken(
        int id,
        int orderId,
        string token,
        OrderStatus targetStatus,
        bool used,
        DateTime expiresAt,
        DateTime? createdAt = null)
    {
        public int Id { get; init; } = id;
        public int OrderId { get; init; } = orderId;
        public string Token { get; init; } = token;
        public OrderStatus TargetStatus { get; init; } = targetStatus;
        public bool Used { get; set; } = used;
        public DateTime ExpiresAt { get; init; } = expiresAt;
        public DateTime? CreatedAt { get; init; } = createdAt;

        public override string ToString() =>
            $"OrderStatusToken {Id}: OrderId: {OrderId}, TargetStatus: {TargetStatus}, " +
            $"Used: {Used}, ExpiresAt: {ExpiresAt}, CreatedAt: {CreatedAt}";
    }
}
