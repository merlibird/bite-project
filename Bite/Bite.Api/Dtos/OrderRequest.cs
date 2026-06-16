namespace Bite.Api.Dtos;

public record OrderRequest
{
    public List<OrderItemDto> Items { get; init; } = [];
    public RestaurantAddressDto DeliveryAddress { get; init; } = new();
}

public record OrderItemDto
{
    public int MenuItemId { get; init; }
    public int Quantity { get; init; }
}
