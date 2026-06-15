namespace Bite.Services.Interface;

public sealed record WebhookJob(string WebhookUrl, object Payload, string? Description = null);

public interface IWebhookQueue
{
    void Enqueue(WebhookJob job);

    WebhookJob? Dequeue();
}
