using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Restaurant(
        int id,
        string name,
        int addressId,
        string webhookUrl,
        string apiKey,
        string? titleImagePath = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        public int Id { get; init; } = id;
        public string Name { get; set; } = name;
        public int AddressId { get; set; } = addressId;
        public string WebhookUrl { get; set; } = webhookUrl;
        public string ApiKey { get; init; } = apiKey;
        public string? TitleImagePath { get; set; } = titleImagePath;
        public DateTime? CreatedAt { get; init; } = createdAt;
        public DateTime? UpdatedAt { get; init; } = updatedAt;

        public override string? ToString() =>
            $"Restaurant {Id}: {Name} (AddressId: {AddressId}), WebhookUrl: {WebhookUrl}, " +
            (TitleImagePath != null ? $"TitleImagePath: {TitleImagePath}" : "") +
            (CreatedAt.HasValue ? $", CreatedAt: {CreatedAt}" : "") +
            (UpdatedAt.HasValue ? $", UpdatedAt: {UpdatedAt}" : "");
    }
}
