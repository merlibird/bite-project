using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;

public class MenuCategory(
    int id,
    int restaurantId,
    string name,
    bool isActive = true)
{
    public int Id { get; init; } = id;
    public int RestaurantId { get; init; } = restaurantId;
    public string Name { get; set; } = name;
    public bool IsActive { get; set; } = isActive;

    public override string ToString()
    {
        return $"MenuCategory {Id}: {Name} (RestaurantId: {RestaurantId}, IsActive: {IsActive})";
    }
}
