using System.ComponentModel.DataAnnotations;

namespace Bite.Api.Dtos;

public record DeliveryFeeRuleDto
{
    [Range(0, 100000)]
    public decimal MaxOrderValue { get; init; }

    [Range(0, 100000)]
    public decimal DeliveryFee { get; init; }
}
