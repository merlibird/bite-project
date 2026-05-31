using Bite.Domain;
using Bite.Api.Dtos;

namespace Bite.Api.Dtos.Mappers;

public static class MenuMapper
{
    public static MenuDto ToMenuDto(this Menu menu)
    {
        return new MenuDto
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