using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class StatusHistoryEntryDao(IConnectionFactory connectionFactory) : IStatusHistoryEntryDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    public async Task<int> InsertAsync(StatusHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        // timestamp is set by the database default (GETDATE()).
        return await template.QuerySingleAsync(
            """
            insert into StatusHistoryEntry
            (order_id, old_status, new_status)
            output inserted.id
            values
            (@orderId, @oldStatus, @newStatus)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@orderId", entry.OrderId),
            new QueryParameter("@oldStatus", entry.OldStatus?.ToDbValue()),
            new QueryParameter("@newStatus", entry.NewStatus.ToDbValue())
            ],
            cancellationToken);
    }
}
