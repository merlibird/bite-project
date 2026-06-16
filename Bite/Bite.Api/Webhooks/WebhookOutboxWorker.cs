using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;

namespace Bite.Api.Webhooks;

public sealed class WebhookOutboxWorker(
    IServiceScopeFactory scopeFactory,
    IWebhookSender sender,
    ILogger<WebhookOutboxWorker> logger) : BackgroundService
{
    // Delay before each attempt
    private static readonly int[] RetryDelaysSeconds = [0, 2, 5, 15, 30, 60];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processing pass failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task ProcessDueAsync(CancellationToken stoppingToken)
    {
        // This is a singleton background service, so we create a scope to get scoped services
        using var scope = scopeFactory.CreateScope();
        var outboxDao = scope.ServiceProvider.GetRequiredService<IWebhookOutboxDao>();
        var orderDao = scope.ServiceProvider.GetRequiredService<ICustomerOrderDao>();

        var dueEntries = await outboxDao.FindDueAsync(maxRows: 20, stoppingToken);

        foreach (var entry in dueEntries)
        {
            if (await sender.SendAsync(entry.Url, entry.Payload, stoppingToken))
            {
                await outboxDao.MarkSentAsync(entry.Id, stoppingToken);

                // Move status from RECEIVED to SENT_TO_RESTAURANT
                await orderDao.UpdateStatusIfAsync(entry.OrderId, OrderStatus.Received, OrderStatus.SentToRestaurant, stoppingToken);

                logger.LogInformation("Outbox {Id} delivered for order {OrderId}.", entry.Id, entry.OrderId);
            }
            else
            {
                int attempts = entry.Attempts + 1;
                if (attempts < RetryDelaysSeconds.Length)
                {
                    await outboxDao.RescheduleAsync(entry.Id, attempts, RetryDelaysSeconds[attempts], stoppingToken);
                    logger.LogWarning("Outbox {Id} failed (attempt {Attempts}), will retry.", entry.Id, attempts);
                }
                else
                {
                    await outboxDao.MarkFailedAsync(entry.Id, attempts, stoppingToken);
                    logger.LogError("Outbox {Id} permanently failed for order {OrderId}.", entry.Id, entry.OrderId);
                }
            }
        }
    }
}
