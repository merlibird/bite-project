using Bite.Services.Interface;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Bite.Services.Implementation;

public class ApiKeyService : IApiKeyService
{
    private const int ApiKeyByteLength = 32;

    public string GenerateApiKey()
        => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(ApiKeyByteLength));

    public string HashApiKey(string apiKey)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
}
