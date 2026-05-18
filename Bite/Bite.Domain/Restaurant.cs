using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Restaurant(
        int id,
        string name,
        int addressId,
        string? webhookUrl,
        string? titleImagePath,
        DateTime createdAt,
        DateTime updatedAt)
    {
        public int Id { get; init; } = id;
        public string Name { get; set; } = name;
        public int AddressId { get; set; } = addressId;
        public string? WebhookUrl { get; set; } = webhookUrl;
        public string? TitleImagePath { get; set; } = titleImagePath;
        public DateTime CreatedAt { get; init; } = createdAt;
        public DateTime UpdatedAt { get; init; } = updatedAt;

        public override string? ToString()
        {
            return $"Restaurant {Id}: {Name} (AddressId: {AddressId}), CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}";
        }
    }
}
