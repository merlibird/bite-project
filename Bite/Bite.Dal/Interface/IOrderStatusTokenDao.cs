using Bite.Domain;

using System.Threading;

namespace Bite.Dal.Interface;

public interface IOrderStatusTokenDao
{
    Task<int> InsertAsync(OrderStatusToken token, CancellationToken cancellationToken = default);

    Task<OrderStatusToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<bool> MarkUsedAsync(int id, CancellationToken cancellationToken = default);
}
