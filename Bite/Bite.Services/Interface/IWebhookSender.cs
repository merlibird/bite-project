namespace Bite.Services.Interface;

// webhook logic was created with the help of AI
public interface IWebhookSender
{
    Task<bool> SendAsync(string webhookUrl, string jsonPayload, CancellationToken cancellationToken = default);
}
