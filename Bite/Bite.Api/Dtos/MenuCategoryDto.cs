namespace Bite.Api.Dtos;


public record MenuCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public List<MenuItemDto> Items { get; init; } = [];
}

