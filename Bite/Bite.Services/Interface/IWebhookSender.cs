namespace Bite.Services.Interface;

public interface IWebhookSender
{
    Task<bool> SendAsync(string webhookUrl, string jsonPayload, CancellationToken cancellationToken = default);
}
