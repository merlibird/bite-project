namespace Bite.Services.Interface;

public interface IApiKeyService
{
    string GenerateApiKey();
    string HashApiKey(string apiKey);
}
