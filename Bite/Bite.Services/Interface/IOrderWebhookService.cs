using Bite.Domain;

namespace Bite.Services.Interface;

// webhook logic was created with the help of AI
public sealed record OrderWebhookPayload(
    string OrderCode,
    IReadOnlyList<OrderWebhookItem> Items,
    OrderWebhookAddress DeliveryAddress,
    decimal DeliveryFee,
    decimal Total,
    IReadOnlyDictionary<string, string> StatusLinks);

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
        IReadOnlyDictionary<OrderStatus, string> statusTokens,
        CancellationToken cancellationToken = default);
}
