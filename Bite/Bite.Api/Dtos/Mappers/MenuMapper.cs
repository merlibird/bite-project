using Bite.Domain;
using Bite.Api.Dtos;

namespace Bite.Api.Dtos.Mappers;

public static class MenuMapper
{
    public static Menu ToMenu(this UpdateMenuRequest request, int restaurantId)
    {
        return new Menu(
            restaurantId: restaurantId,
            categories: (request.Categories ?? [])
                .Select(category => new MenuCategoryWithItems(
                    id: 0,
                    name: category.Name,
                    items: (category.Items ?? [])
                        .Select(item => new MenuItem(
                            id: 0,
                            restaurantId: restaurantId,
                            name: item.Name,
                            description: item.Description,
                            price: item.Price,
                            isActive: item.IsActive))
                        .ToList()))
                .ToList());
    }

    public static MenuResponse ToMenuResponse(this Menu menu)
    {
        return new MenuResponse
        {
            RestaurantId = menu.RestaurantId,
            Categories = menu.Categories
                .Select(category => new MenuCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Items = category.Items
                        .Select(item => new MenuItemDto
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Price = item.Price,
                            IsActive = item.IsActive
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
