using Bite.Services.Interface;
using System.Security.Cryptography;
using System.Text;

namespace Bite.Services.Implementation;

public class ApiKeyService : IApiKeyService
{
    public string GenerateApiKey()
        => Guid.NewGuid().ToString();

    public string HashApiKey(string apiKey)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
}
