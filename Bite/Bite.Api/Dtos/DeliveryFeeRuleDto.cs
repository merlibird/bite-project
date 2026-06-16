namespace Bite.Api.Dtos;

public record DeliveryFeeRuleDto
{
    public decimal MaxOrderValue { get; init; }
    public decimal DeliveryFee { get; init; }
}
