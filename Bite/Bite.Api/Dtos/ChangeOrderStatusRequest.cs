using Bite.Domain;

namespace Bite.Api.Dtos;

public record ChangeOrderStatusRequest
{
    public required OrderStatus Status { get; init; }
}
