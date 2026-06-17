using Bite.Domain;

namespace Bite.Services.Interface;

public interface IOrderStatusTokenService
{
    Task<IReadOnlyDictionary<OrderStatus, string>> CreateTokensForOrderAsync(int orderId, CancellationToken cancellationToken = default);
}
