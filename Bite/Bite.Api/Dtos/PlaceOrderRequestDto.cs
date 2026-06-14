namespace Bite.Api.Dtos;

public record PlaceOrderRequestDto
{
    public List<OrderItemRequestDto> Items { get; init; } = [];
    public RestaurantAddressDto DeliveryAddress { get; init; } = new();
}
