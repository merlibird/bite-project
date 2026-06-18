using Bite.Domain;

namespace Bite.Api.Dtos;

public record OrderStatusResponse
{
    public required string OrderCode { get; init; }
    public required OrderStatus Status { get; init; }
}
