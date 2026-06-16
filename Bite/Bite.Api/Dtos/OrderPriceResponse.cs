namespace Bite.Api.Dtos;

public record OrderPriceResponse
{
    public decimal Subtotal { get; init; }
    public decimal DeliveryFee { get; init; }
    public decimal TotalPrice => Subtotal + DeliveryFee;
}
