namespace Bite.Api.Dtos;

public record ChangeOrderStatusRequest
{
    public required string Status { get; init; }
}
