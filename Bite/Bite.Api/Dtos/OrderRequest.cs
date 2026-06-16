using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record OrderRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
    public List<OrderItemDto> Items { get; init; } = [];

    [Required]
    public RestaurantAddressDto DeliveryAddress { get; init; } = new();
}

public record OrderItemDto
{
    [Range(1, int.MaxValue)]
    public int MenuItemId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; init; }
}
