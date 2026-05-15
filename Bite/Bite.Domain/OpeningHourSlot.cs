using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class OpeningHourSlot(
        int id,
        int restaurantId,
        int dayOfWeek,
        TimeSpan openTime,
        TimeSpan closeTime)
    {
        public int Id { get; init; } = id;
        public int RestaurantId { get; init; } = restaurantId;
        public int DayOfWeek { get; set; } = dayOfWeek;  // 0=So, 1=Mo, ..., 6=Sa
        public TimeSpan OpenTime { get; set; } = openTime;
        public TimeSpan CloseTime { get; set; } = closeTime;

        public override string ToString() 
        {
            return $"Id: {Id}, RestaurantId: {RestaurantId}, DayOfWeek: {DayOfWeek}, OpenTime: {OpenTime}, CloseTime: {CloseTime}";
        }
    }
}
