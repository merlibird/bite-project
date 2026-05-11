using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MenuId { get; set; }
        public int AddressId { get; set; }
        public string WebhookUrl { get; set; } = string.Empty;
        public string TitleImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation (optional, wird nicht immer befüllt)
        public Address? Address { get; set; }
        public List<OpeningHourSlot> OpeningHours { get; set; } = new();
    }
}
