using Bite.Domain;
using Bite.Services.Common;

namespace Bite.Services.Interface;

public interface IOrderService
{
    Task<ServiceResult<OrderStatus>> GetStatusAsync(string orderCode, CancellationToken cancellationToken = default);

    Task<ServiceResult<OrderStatus>> ChangeStatusAsync(string orderCode, int restaurantId, OrderStatus newStatus, CancellationToken cancellationToken = default);

    Task<ServiceResult<OrderStatus>> ApplyStatusTokenAsync(string orderCode, string token, int restaurantId, CancellationToken cancellationToken = default);

    Task<ServiceResult<OrderPriceResponse>> CalculatePriceAsync(int restaurantId, IEnumerable<(int MenuItemId, int Quantity)> items, (double Latitude, double Longitude) deliveryLocation, CancellationToken cancellationToken = default);

    Task<ServiceResult<string>> PlaceOrderAsync(int restaurantId, IEnumerable<(int MenuItemId, int Quantity)> items, Address deliveryAddress, CancellationToken cancellationToken = default);
}

public record OrderPriceResponse(decimal Subtotal, decimal DeliveryFee, decimal TotalPrice);
