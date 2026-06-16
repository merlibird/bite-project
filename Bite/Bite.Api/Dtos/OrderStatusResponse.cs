namespace Bite.Api.Dtos;

public record OrderStatusResponse
{
    public required string OrderCode { get; init; }
    public required string Status { get; init; }
}
