using Bite.Domain;

namespace Bite.Dal.Interface;

public interface IWebhookOutboxDao
{
    Task InsertAsync(int orderId, string url, string payload, CancellationToken cancellationToken = default);

    Task<IEnumerable<WebhookOutboxEntry>> FindDueAsync(int maxRows, CancellationToken cancellationToken = default);

    Task MarkSentAsync(int id, CancellationToken cancellationToken = default);

    Task RescheduleAsync(int id, int attempts, int delaySeconds, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(int id, int attempts, CancellationToken cancellationToken = default);
}
