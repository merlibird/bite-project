using Bite.Domain;

using System.Threading;

namespace Bite.Dal.Interface;

public interface IOrderItemDao
{
    Task<int> InsertAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

    Task<IEnumerable<OrderItem>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
}
