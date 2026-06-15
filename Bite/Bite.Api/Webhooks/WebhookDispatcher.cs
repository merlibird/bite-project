using Bite.Services.Interface;

namespace Bite.Api.Webhooks;

public sealed class WebhookDispatcher(
    IWebhookQueue queue,
    IWebhookSender sender,
    ILogger<WebhookDispatcher> logger) : BackgroundService
{
    private static readonly int[] RetryDelaysSeconds = [0, 2, 5, 15];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Deliver everything currently in the queue...
            while (queue.Dequeue() is { } job)
            {
                await DeliverWithRetryAsync(job, stoppingToken);
            }

            // ...then wait a second and check again.
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task DeliverWithRetryAsync(WebhookJob job, CancellationToken stoppingToken)
    {
        foreach (int delaySeconds in RetryDelaysSeconds)
        {
            if (delaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);

            if (await sender.SendAsync(job.WebhookUrl, job.Payload, stoppingToken))
            {
                logger.LogInformation("Webhook delivered to {Url} ({Desc}).", job.WebhookUrl, job.Description);
                return;
            }

            logger.LogWarning("Webhook to {Url} failed, will retry ({Desc}).", job.WebhookUrl, job.Description);
        }

        logger.LogError("Webhook permanently failed to {Url} ({Desc}).", job.WebhookUrl, job.Description);
    }
}
