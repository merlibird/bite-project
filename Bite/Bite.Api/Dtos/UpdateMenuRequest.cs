using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public class UpdateMenuRequest
{
    public List<UpdateMenuCategoryRequest> Categories { get; set; } = [];
}

public class UpdateMenuCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public List<UpdateMenuItemRequest> Items { get; set; } = [];
}

public class UpdateMenuItemRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
