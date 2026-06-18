using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Bite.Services.Implementation;

// webhook logic was created with the help of AI
public sealed class OrderWebhookService(
    IRestaurantDao restaurantDao,
    IWebhookOutboxDao outboxDao,
    IConfiguration configuration) : IOrderWebhookService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task NotifyOrderCreatedAsync(
        CustomerOrder order,
        Address deliveryAddress,
        IReadOnlyList<(MenuItem MenuItem, int Quantity)> items,
        IReadOnlyDictionary<OrderStatus, string> statusTokens,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(order.RestaurantId, cancellationToken);
        if (restaurant is null || string.IsNullOrWhiteSpace(restaurant.WebhookUrl))
        {
            return; // no webhook configured -> nothing to enqueue
        }

        string baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5126";

        var statusLinks = statusTokens.ToDictionary(
            kvp => kvp.Key.ToDbValue(),
            kvp => $"{baseUrl}/api/Orders/{order.OrderCode}/status-change/{kvp.Value}"
        );

        var payload = new OrderWebhookPayload(
            order.OrderCode,
            items.Select(i => new OrderWebhookItem(
                i.MenuItem.Id,
                i.MenuItem.Name,
                i.Quantity,
                i.MenuItem.Price)).ToList(),
            new OrderWebhookAddress(
                deliveryAddress.Street,
                deliveryAddress.Number,
                deliveryAddress.ZipCode,
                deliveryAddress.City,
                deliveryAddress.Country,
                deliveryAddress.AdditionalInfo),
            order.DeliveryFee,
            order.Total,
            statusLinks);

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        await outboxDao.InsertAsync(order.Id, restaurant.WebhookUrl, json, cancellationToken);
    }
}
