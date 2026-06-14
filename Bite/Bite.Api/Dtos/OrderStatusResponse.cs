namespace Bite.Api.Dtos;

public class OrderStatusResponse
{
    public required string OrderCode { get; init; }
    public required string Status { get; init; }
}
