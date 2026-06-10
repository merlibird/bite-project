using Bite.Services.Interface;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace Bite.Services.Implementation;

public class OrderStatusTokenService : IOrderStatusTokenService
{
    private const int TokenByteLength = 32;

    public string GenerateToken()
        => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenByteLength));
}
