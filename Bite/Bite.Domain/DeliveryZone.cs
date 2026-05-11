using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class DeliveryZone
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public decimal MinOrderValue { get; set; }
        public double MaxDistance { get; set; }  // in km

        public List<DeliveryFeeRule> FeeRules { get; set; } = new();
    }
}
