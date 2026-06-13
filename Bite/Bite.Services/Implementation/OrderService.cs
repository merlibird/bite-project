using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Interface;

namespace Bite.Services.Implementation;

public class OrderService(
    ICustomerOrderDao customerOrderDao) : IOrderService
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
}
