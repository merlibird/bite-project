using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class StatusHistoryEntry(
        int id,
        int orderId,
        OrderStatus newStatus,
        OrderStatus? oldStatus = null,
        DateTime? timestamp = null)
    {
        public int Id { get; init; } = id;
        public int OrderId { get; init; } = orderId;
        public OrderStatus? OldStatus { get; init; } = oldStatus;
        public OrderStatus NewStatus { get; init; } = newStatus;
        public DateTime? Timestamp { get; init; } = timestamp;

        public override string ToString() =>
            $"StatusHistoryEntry {Id}: OrderId: {OrderId}, " +
            $"{OldStatus?.ToString() ?? "—"} -> {NewStatus} at {Timestamp}";
    }
}
