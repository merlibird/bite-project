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
}
