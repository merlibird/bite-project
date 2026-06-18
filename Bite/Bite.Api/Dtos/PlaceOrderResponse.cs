namespace Bite.Api.Dtos;

public record PlaceOrderResponse
{
    public string OrderCode { get; init; } = string.Empty;
}
