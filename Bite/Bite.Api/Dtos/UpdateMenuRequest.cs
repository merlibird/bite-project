using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record UpdateMenuRequest
{
    public List<UpdateMenuCategoryDto> Categories { get; init; } = [];
}

public record UpdateMenuCategoryDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    public List<UpdateMenuItemDto> Items { get; init; } = [];
}

public record UpdateMenuItemDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; init; }

    [Range(0, 100000)]
    public decimal Price { get; init; }

    public bool IsActive { get; init; } = true;
}
