using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class DeliveryFeeRule(
        int id,
        int deliveryZoneId,
        decimal maxOrderValue,
        decimal deliveryFee)
    {
        public int Id { get; init; } = id;
        public int DeliveryZoneId { get; init; } = deliveryZoneId;
        public decimal MaxOrderValue { get; set; } = maxOrderValue;
        public decimal DeliveryFee { get; set; } = deliveryFee;

        public override string ToString()
        {
            return $"DeliveryFeeRule {Id}: DeliveryZoneId: {DeliveryZoneId}, MaxOrderValue: {MaxOrderValue}, DeliveryFee: {DeliveryFee}";
        }
    }
}
