namespace Bite.Api.Dtos;

public record PlaceOrderResponseDto
{
    public string OrderCode { get; init; } = string.Empty;
}
