using Bite.Domain;
using Bite.Services.Common;

namespace Bite.Services.Interface;

public interface IOrderService
{
    Task<ServiceResult<OrderStatus>> GetStatusAsync(string orderCode, CancellationToken cancellationToken = default);
}
