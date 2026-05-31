namespace Bite.Api.Dtos;
public record MenuDto
{
    public int RestaurantId { get; set; }
    public List<MenuCategoryDto> Categories { get; set; } = [];
}

