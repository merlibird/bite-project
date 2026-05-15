using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class MenuItem(
        int id,
        int restaurantId,
        int categoryId,
        string name,
        string? description,
        decimal price,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        public int Id { get; init; } = id;
        public int RestaurantId { get; init; } = restaurantId;
        public int CategoryId { get; set; } = categoryId;
        public string Name { get; init; } = name;
        public string? Description { get; set; } = description;
        public decimal Price { get; init; } = price;
        public bool IsActive { get; set; } = isActive;
        public DateTime CreatedAt { get; init; } = createdAt;
        public DateTime UpdatedAt { get; init; } = updatedAt;

        public override string ToString()
        {
            return $"MenuItem {Id}: {Name} (RestaurantId: {RestaurantId}, CategoryId: {CategoryId}), Price: {Price}, IsActive: {isActive}, CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}";
        }
    }
}
