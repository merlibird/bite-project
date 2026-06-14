namespace Bite.Api.Dtos;

public record OrderPriceRequestDto
{
    public List<OrderItemRequestDto> Items { get; init; } = [];
    public RestaurantAddressDto DeliveryAddress { get; init; } = new();
}

public record OrderItemRequestDto
{
    public int MenuItemId { get; init; }
    public int Quantity { get; init; }
}
