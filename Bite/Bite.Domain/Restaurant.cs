using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Restaurant(
        int id,
        string name,
        int menuId,
        int addressId,
        string webhookUrl,
        string titleImagePath)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public int MenuId { get; set; } = menuId;
        public int AddressId { get; set; } = addressId;
        public string WebhookUrl { get; set; } = webhookUrl;
        public string TitleImagePath { get; set; } = titleImagePath;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation (optional, wird nicht immer befüllt)
        public Address? Address { get; set; }
        public List<OpeningHourSlot> OpeningHours { get; set; } = new();

        public override string? ToString()
        {
            return $"Restaurant {Id}: {Name} (MenuId: {MenuId}, AddressId: {AddressId})";
        }
    }
}
