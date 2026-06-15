namespace Bite.Services.Interface;

public interface IWebhookSender
{
    Task<bool> SendAsync(string webhookUrl, object payload, CancellationToken cancellationToken = default);
}
