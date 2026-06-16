namespace Bite.Api.Dtos;


public record MenuCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public List<MenuItemDto> Items { get; set; } = [];
}

