using Bite.Domain;

using System.Threading;

namespace Bite.Dal.Interface;

public interface ICustomerOrderDao
{
    Task<int> InsertAsync(CustomerOrder order, CancellationToken cancellationToken = default);

    Task<CustomerOrder?> FindByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default);
}
