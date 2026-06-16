namespace Bite.Api.Dtos;

public record DeliveryZoneDto
{
    public double MaxDistance { get; init; }
    public decimal MinOrderValue { get; init; }
    public List<DeliveryFeeRuleDto> FeeRules { get; init; } = [];
}
