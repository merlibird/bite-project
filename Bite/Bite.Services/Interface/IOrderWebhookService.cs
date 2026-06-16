using Bite.Domain;

namespace Bite.Services.Interface;

public sealed record OrderWebhookPayload(
    string OrderCode,
    IReadOnlyList<OrderWebhookItem> Items,
    OrderWebhookAddress DeliveryAddress,
    decimal DeliveryFee,
    decimal Total);

public sealed record OrderWebhookItem(
    int MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderWebhookAddress(
    string Street,
    string Number,
    string ZipCode,
    string City,
    string Country,
    string? AdditionalInfo);

public interface IOrderWebhookService
{
    Task NotifyOrderCreatedAsync(
        CustomerOrder order,
        Address deliveryAddress,
        IReadOnlyList<(MenuItem MenuItem, int Quantity)> items,
        CancellationToken cancellationToken = default);
}
