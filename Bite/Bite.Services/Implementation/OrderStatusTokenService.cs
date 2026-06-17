using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Bite.Services.Implementation;

public class OrderStatusTokenService(IOrderStatusTokenDao orderStatusTokenDao) : IOrderStatusTokenService
{
    private const int TokenByteLength = 32;
    private const int ValidforNDays = 7;

    public async Task<IReadOnlyDictionary<OrderStatus, string>> CreateTokensForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var statusTransitions = new[]
        {
            OrderStatus.SentToRestaurant,
            OrderStatus.InPreparation,
            OrderStatus.OutForDelivery,
            OrderStatus.Delivered,
            OrderStatus.Cancelled
        };

        var tokens = new Dictionary<OrderStatus, string>();

        foreach (var status in statusTransitions)
        {
            string tokenValue = GenerateToken();
            var statusToken = new OrderStatusToken(
                id: 0,
                orderId: orderId,
                token: tokenValue,
                targetStatus: status,
                used: false,
                expiresAt: DateTime.UtcNow.AddDays(ValidforNDays) // Tokens valid for 7 days
            );

            await orderStatusTokenDao.InsertAsync(statusToken, cancellationToken);
            tokens[status] = tokenValue;
        }

        return tokens;
    }

    private string GenerateToken()
        => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenByteLength));
}
