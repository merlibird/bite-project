using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record UpdateMenuRequest
{
    public List<UpdateMenuCategoryDto> Categories { get; init; } = [];
}

public record UpdateMenuCategoryDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public List<UpdateMenuItemDto> Items { get; init; } = [];
}

public record UpdateMenuItemDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public decimal Price { get; init; }

    public bool IsActive { get; init; } = true;
}
