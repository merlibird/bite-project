using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class DeliveryFeeRule
    {
        public int Id { get; set; }
        public int DeliveryZoneId { get; set; }
        public decimal MaxOrderValue { get; set; }
        public decimal DeliveryFee { get; set; }
    }
}
