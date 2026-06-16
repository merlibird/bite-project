using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record UpdateMenuRequest
{
    public List<UpdateMenuCategoryDto> Categories { get; init; } = [];
}

public record UpdateMenuCategoryDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<UpdateMenuItemRequest> Items { get; set; } = [];
}

public record UpdateMenuItemDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; init; }

    [Range(0, 100000)]
    public decimal Price { get; init; }

    public bool IsActive { get; init; } = true;
}
