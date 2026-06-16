using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class WebhookOutboxDao(IConnectionFactory connectionFactory) : IWebhookOutboxDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private WebhookOutboxEntry MapRow(IDataRecord row) => new(
        (int)row["id"],
        (int)row["order_id"],
        (string)row["url"],
        (string)row["payload"],
        (int)row["attempts"]);

    public async Task InsertAsync(int orderId, string url, string payload, CancellationToken cancellationToken = default)
    {
        await template.ExecuteAsync(
            """
            insert into WebhookOutbox (order_id, url, payload)
            values (@orderId, @url, @payload)
            """,
            [
            new QueryParameter("@orderId", orderId),
            new QueryParameter("@url", url),
            new QueryParameter("@payload", payload)
            ],
            cancellationToken);
    }

    public async Task<IEnumerable<WebhookOutboxEntry>> FindDueAsync(int maxRows, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            """
            select top (@maxRows) id, order_id, url, payload, attempts
            from WebhookOutbox
            where status = 'PENDING' and next_attempt_at <= GETUTCDATE()
            order by next_attempt_at
            """,
            MapRow,
            [new QueryParameter("@maxRows", maxRows)],
            cancellationToken);
    }

    public async Task MarkSentAsync(int id, CancellationToken cancellationToken = default)
    {
        await template.ExecuteAsync(
            "update WebhookOutbox set status = 'SENT' where id = @id",
            [new QueryParameter("@id", id)],
            cancellationToken);
    }

    public async Task RescheduleAsync(int id, int attempts, int delaySeconds, CancellationToken cancellationToken = default)
    {
        await template.ExecuteAsync(
            """
            update WebhookOutbox
            set attempts = @attempts,
                next_attempt_at = DATEADD(SECOND, @delaySeconds, GETUTCDATE())
            where id = @id
            """,
            [
            new QueryParameter("@attempts", attempts),
            new QueryParameter("@delaySeconds", delaySeconds),
            new QueryParameter("@id", id)
            ],
            cancellationToken);
    }

    public async Task MarkFailedAsync(int id, int attempts, CancellationToken cancellationToken = default)
    {
        await template.ExecuteAsync(
            "update WebhookOutbox set status = 'FAILED', attempts = @attempts where id = @id",
            [
            new QueryParameter("@attempts", attempts),
            new QueryParameter("@id", id)
            ],
            cancellationToken);
    }
}
