namespace Bite.Api.Dtos;
public record MenuResponse
{
    public int RestaurantId { get; init; }
    public List<MenuCategoryDto> Categories { get; init; } = [];
}
