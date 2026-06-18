using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;

public class Menu(
      int restaurantId,
      IEnumerable<MenuCategoryWithItems> categories)
{
    public int RestaurantId { get; init; } = restaurantId;
    public IEnumerable<MenuCategoryWithItems> Categories { get; set; } = categories;
}

