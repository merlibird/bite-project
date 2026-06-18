using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record DeliveryZoneDto
{
    [Range(0, 100000)]
    public double MaxDistance { get; init; }

    [Range(0, 100000)]
    public decimal MinOrderValue { get; init; }

    public List<DeliveryFeeRuleDto> FeeRules { get; init; } = [];
}
