using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class OpeningHourSlot
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int DayOfWeek { get; set; }  // 0=So, 1=Mo, ..., 6=Sa
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }
}
