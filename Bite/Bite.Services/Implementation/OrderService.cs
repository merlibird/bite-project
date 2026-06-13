using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;
using System.Transactions;

namespace Bite.Services.Implementation;

public class OrderService(
    ICustomerOrderDao customerOrderDao,
    IOrderStatusTokenDao orderStatusTokenDao) : IOrderService
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
}
