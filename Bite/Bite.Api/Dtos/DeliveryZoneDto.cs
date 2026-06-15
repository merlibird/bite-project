using System.Collections.Generic;

namespace Bite.Api.Dtos;

public class DeliveryZoneDto
{
    public double MaxDistance { get; set; }
    public decimal MinOrderValue { get; set; }
    public List<DeliveryFeeRuleDto> FeeRules { get; set; } = [];
}
