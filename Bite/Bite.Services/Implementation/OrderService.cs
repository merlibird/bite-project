using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using System.Transactions;

namespace Bite.Services.Implementation;

public class OrderService(
    ICustomerOrderDao customerOrderDao,
    IOrderStatusTokenDao orderStatusTokenDao,
    IRestaurantDao restaurantDao,
    IAddressDao addressDao,
    IMenuItemDao menuItemDao,
    IDeliveryZoneDao deliveryZoneDao,
    IDeliveryFeeRuleDao deliveryFeeRuleDao) : IOrderService
{
    public async Task<ServiceResult<OrderStatus>> GetStatusAsync(string orderCode, CancellationToken cancellationToken = default)
    {
        var order = await customerOrderDao.FindByOrderCodeAsync(orderCode, cancellationToken);
        if (order == null)
        {
            return ServiceResult<OrderStatus>.Failure(
                $"No order found with code '{orderCode}'.",
                ServiceResultType.NotFound);
        }

        return ServiceResult<OrderStatus>.Success(order.Status);
    }

    public async Task<ServiceResult<OrderStatus>> ChangeStatusAsync(string orderCode, int restaurantId, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await customerOrderDao.FindByOrderCodeAsync(orderCode, cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderStatus>.Failure(
                $"No order found with code '{orderCode}'.",
                ServiceResultType.NotFound);
        }

        // Check if the order belongs to the restaurant making the request
        if (order.RestaurantId != restaurantId)
        {
            return ServiceResult<OrderStatus>.Failure(
                "This order does not belong to your restaurant.",
                ServiceResultType.Forbidden);
        }

        var updated = await customerOrderDao.UpdateStatusAsync(order.Id, newStatus, cancellationToken);
        if (!updated)
        {
            return ServiceResult<OrderStatus>.Failure(
                "Failed to update the order status.",
                ServiceResultType.Error);
        }

        return ServiceResult<OrderStatus>.Success(newStatus);
    }

    public async Task<ServiceResult<OrderStatus>> ApplyStatusTokenAsync(string orderCode, string token, int restaurantId, CancellationToken cancellationToken = default)
    {
        var order = await customerOrderDao.FindByOrderCodeAsync(orderCode, cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderStatus>.Failure(
                $"No order found with code '{orderCode}'.",
                ServiceResultType.NotFound);
        }

        if (order.RestaurantId != restaurantId)
        {
            return ServiceResult<OrderStatus>.Failure(
                "This order does not belong to your restaurant.",
                ServiceResultType.Forbidden);
        }

        var statusToken = await orderStatusTokenDao.FindByTokenAsync(token, cancellationToken);
        if (statusToken is null || statusToken.OrderId != order.Id)
        {
            return ServiceResult<OrderStatus>.Failure(
                "Invalid status-change token.",
                ServiceResultType.NotFound);
        }

        if (statusToken.Used)
        {
            return ServiceResult<OrderStatus>.Failure(
                "This status-change link has already been used.",
                ServiceResultType.Conflict);
        }

        if (statusToken.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult<OrderStatus>.Failure(
                "This status-change link has expired.",
                ServiceResultType.Conflict);
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // Atomically claim the token; if another request consumed it first, this returns false.
        var claimed = await orderStatusTokenDao.MarkUsedAsync(statusToken.Id, cancellationToken);
        if (!claimed)
        {
            return ServiceResult<OrderStatus>.Failure(
                "This status-change link has already been used.",
                ServiceResultType.Conflict);
        }

        await customerOrderDao.UpdateStatusAsync(order.Id, statusToken.TargetStatus, cancellationToken);

        scope.Complete();

        return ServiceResult<OrderStatus>.Success(statusToken.TargetStatus);
    }

    public async Task<ServiceResult<OrderPriceResponse>> CalculatePriceAsync(
        int restaurantId,
        IEnumerable<(int MenuItemId, int Quantity)> items,
        (double Latitude, double Longitude) deliveryLocation,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return ServiceResult<OrderPriceResponse>.Failure("Restaurant not found.", ServiceResultType.NotFound);
        }

        var restaurantAddress = await addressDao.FindByIdAsync(restaurant.AddressId, cancellationToken);
        if (restaurantAddress is null)
        {
            return ServiceResult<OrderPriceResponse>.Failure("Restaurant address not found.", ServiceResultType.Error);
        }

        decimal subtotal = 0;
        foreach (var itemRequest in items)
        {
            var menuItem = await menuItemDao.FindByIdAsync(itemRequest.MenuItemId, cancellationToken);
            if (menuItem is null || menuItem.RestaurantId != restaurantId || !menuItem.IsActive)
            {
                return ServiceResult<OrderPriceResponse>.Failure($"Invalid or inactive menu item: {itemRequest.MenuItemId}", ServiceResultType.Error);
            }
            subtotal += menuItem.Price * itemRequest.Quantity;
        }

        double distance = GeoUtils.CalculateDistanceInKm(
            restaurantAddress.Latitude,
            restaurantAddress.Longitude,
            deliveryLocation.Latitude,
            deliveryLocation.Longitude);

        var zones = (await deliveryZoneDao.FindByRestaurantIdAsync(restaurantId, cancellationToken))
            .Where(z => distance <= z.MaxDistance)
            .OrderBy(z => z.MaxDistance)
            .ToList();

        if (zones.Count == 0)
        {
            return ServiceResult<OrderPriceResponse>.Failure("Outside delivery area.", ServiceResultType.Error);
        }

        var validZone = zones.FirstOrDefault(z => subtotal >= z.MinOrderValue);
        if (validZone is null)
        {
            return ServiceResult<OrderPriceResponse>.Failure("Minimum order value not reached.", ServiceResultType.Error);
        }

        var rules = (await deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(restaurantId, validZone.Id, cancellationToken))
            .OrderBy(r => r.MaxOrderValue)
            .ToList();

        decimal deliveryFee = 0;
        var applicableRule = rules.FirstOrDefault(r => subtotal <= r.MaxOrderValue);

        if (applicableRule is not null)
        {
            deliveryFee = applicableRule.DeliveryFee;
        }
        else if (rules.Count > 0)
        {
            // Fallback: Wenn Zwischensumme größer als alle MaxOrderValues, wird standardmäßig der Wert 0 verwendet.
            // TODO: weiter besprechen: sollte es die Gebühr der höchsten Stufe oder 0 sein?
            // Wie versteht Tarik das?l
            deliveryFee = 0;
        }

        return ServiceResult<OrderPriceResponse>.Success(new OrderPriceResponse(subtotal, deliveryFee, subtotal + deliveryFee));
    }
}
