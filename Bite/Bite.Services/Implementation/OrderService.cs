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
    IDeliveryFeeRuleDao deliveryFeeRuleDao,
    IOrderCodeService orderCodeService,
    IOrderItemDao orderItemDao,
    IOrderWebhookService orderWebhookService,
    IOrderStatusTokenService orderStatusTokenService) : IOrderService
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

        if (!order.Status.CanTransitionTo(newStatus))
        {
            return ServiceResult<OrderStatus>.Failure(
                $"Cannot transition from {order.Status} to {newStatus}.",
                ServiceResultType.ValidationError);
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

        if (!order.Status.CanTransitionTo(statusToken.TargetStatus))
        {
            return ServiceResult<OrderStatus>.Failure(
                $"This status-change link is no longer valid for the current order status ({order.Status}).",
                ServiceResultType.Conflict);
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

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
        var validationResult = await ValidateOrderAsync(restaurantId, items, deliveryLocation, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return ServiceResult<OrderPriceResponse>.Failure(validationResult.ErrorMessage!, validationResult.ResultType);
        }

        var (subtotal, deliveryFee, _) = validationResult.Data;
        return ServiceResult<OrderPriceResponse>.Success(new OrderPriceResponse(subtotal, deliveryFee, subtotal + deliveryFee));
    }

    public async Task<ServiceResult<string>> PlaceOrderAsync(
        int restaurantId,
        IEnumerable<(int MenuItemId, int Quantity)> items,
        Address deliveryAddress,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateOrderAsync(
            restaurantId,
            items,
            (deliveryAddress.Latitude, deliveryAddress.Longitude),
            cancellationToken);

        if (!validationResult.IsSuccess)
        {
            return ServiceResult<string>.Failure(validationResult.ErrorMessage!, validationResult.ResultType);
        }

        var (subtotal, deliveryFee, validatedItems) = validationResult.Data;

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // 1. Insert Address
        int addressId = await addressDao.InsertAsync(deliveryAddress, cancellationToken);

        // 2. Generate Order Code
        string orderCode = orderCodeService.GenerateOrderCode();

        // 3. Insert Order
        var order = new CustomerOrder(
            id: 0,
            restaurantId: restaurantId,
            addressId: addressId,
            orderCode: orderCode,
            status: OrderStatus.Received,
            deliveryFee: deliveryFee,
            total: subtotal + deliveryFee
        );

        int orderId = await customerOrderDao.InsertAsync(order, cancellationToken);

        // 4. Generate Status Tokens for all relevant transitions
        var statusTokens = await orderStatusTokenService.CreateTokensForOrderAsync(orderId, cancellationToken);

        // 5. Insert Order Items
        foreach (var item in validatedItems)
        {
            var orderItem = new OrderItem(
                id: 0,
                orderId: orderId,
                menuItemId: item.MenuItem.Id,
                quantity: item.Quantity,
                unitPrice: item.MenuItem.Price
            );
            await orderItemDao.InsertAsync(orderItem, cancellationToken);
        }

        // 6. Notify via Webhook (add to outbox for async processing)
        var createdOrder = new CustomerOrder(
            id: orderId,
            restaurantId: restaurantId,
            addressId: addressId,
            orderCode: orderCode,
            status: OrderStatus.Received,
            deliveryFee: deliveryFee,
            total: subtotal + deliveryFee);

        await orderWebhookService.NotifyOrderCreatedAsync(createdOrder, deliveryAddress, validatedItems, statusTokens, cancellationToken);

        scope.Complete();

        return ServiceResult<string>.Success(orderCode);
    }

    private async Task<ServiceResult<(decimal Subtotal, decimal DeliveryFee, List<(MenuItem MenuItem, int Quantity)> ValidatedItems)>> ValidateOrderAsync(
        int restaurantId,
        IEnumerable<(int MenuItemId, int Quantity)> items,
        (double Latitude, double Longitude) deliveryLocation,
        CancellationToken cancellationToken)
    {
        var restaurant = await restaurantDao.FindByIdAsync(restaurantId, cancellationToken);
        if (restaurant is null)
        {
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("Restaurant not found.", ServiceResultType.NotFound);
        }

        var restaurantAddress = await addressDao.FindByIdAsync(restaurant.AddressId, cancellationToken);
        if (restaurantAddress is null)
        {
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("Restaurant address not found.", ServiceResultType.Error);
        }

        decimal subtotal = 0;
        var validatedItems = new List<(MenuItem MenuItem, int Quantity)>();

        foreach (var itemRequest in items)
        {
            var menuItem = await menuItemDao.FindByIdAsync(itemRequest.MenuItemId, cancellationToken);
            if (menuItem is null || menuItem.RestaurantId != restaurantId || !menuItem.IsActive)
            {
                return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure($"Invalid or inactive menu item: {itemRequest.MenuItemId}", ServiceResultType.Error);
            }
            subtotal += menuItem.Price * itemRequest.Quantity;
            validatedItems.Add((menuItem, itemRequest.Quantity));
        }

        if (validatedItems.Count == 0)
        {
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("Order must contain at least one item.", ServiceResultType.Error);
        }

        double distance = GeoUtils.CalculateDistanceInKm(
            restaurantAddress.Latitude,
            restaurantAddress.Longitude,
            deliveryLocation.Latitude,
            deliveryLocation.Longitude);

        var zones = (await deliveryZoneDao.FindByRestaurantIdAsync(restaurantId, cancellationToken))
            .Where(z => distance <= z.MaxDistance)
            .OrderBy(z => z.MaxDistance)
            .ThenBy(z => z.MinOrderValue)
            .ToList();

        if (zones.Count == 0)
        {
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("Outside delivery area.", ServiceResultType.ValidationError);
        }

        var validZone = zones.FirstOrDefault(z => subtotal >= z.MinOrderValue);
        if (validZone is null)
        {
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("Minimum order value not reached.", ServiceResultType.ValidationError);
        }

        var rules = (await deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(restaurantId, validZone.Id, cancellationToken))
            .OrderBy(r => r.MaxOrderValue)
            .ToList();

        if (rules.Count == 0)
        {
            // If a zone exists but no rules are defined, it's considered non-deliverable
            return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Failure("No delivery rules defined for this area.", ServiceResultType.ValidationError);
        }

        decimal deliveryFee = 0;
        var applicableRule = rules.FirstOrDefault(r => subtotal <= r.MaxOrderValue);

        if (applicableRule is not null)
        {
            deliveryFee = applicableRule.DeliveryFee;
        }
        else if (rules.Count > 0)
        {
            // Fallback: If subtotal > all MaxOrderValues, default to 0 as in requirements defined.
            deliveryFee = 0;
        }

        return ServiceResult<(decimal, decimal, List<(MenuItem, int)>)>.Success((subtotal, deliveryFee, validatedItems));
    }
}
