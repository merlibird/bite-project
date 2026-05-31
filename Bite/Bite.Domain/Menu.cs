using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain;
public class Menu(
      int restaurantId,
      IEnumerable<MenuCategory> categories)
    {
        public int RestaurantId { get; init; } = restaurantId;
        public IEnumerable<MenuCategory> Categories { get; set; } = categories;
}

