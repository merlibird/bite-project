using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class OrderStatusTokenDao(IConnectionFactory connectionFactory) : IOrderStatusTokenDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private OrderStatusToken MapRowToOrderStatusToken(IDataRecord row)
    {
        return new OrderStatusToken(
            id: (int)row["id"],
            orderId: (int)row["order_id"],
            token: (string)row["token"],
            targetStatus: OrderStatusExtensions.FromDbValue((string)row["target_status"]),
            used: (bool)row["used"],
            expiresAt: (DateTime)row["expires_at"],
            createdAt: (DateTime)row["created_at"]);
    }

    public async Task<OrderStatusToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            "select * from OrderStatusToken where token=@token",
            MapRowToOrderStatusToken,
            [new QueryParameter("@token", token)],
            cancellationToken);
    }

    public async Task<int> InsertAsync(OrderStatusToken token, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into OrderStatusToken
            (order_id, token, target_status, used, expires_at)
            output inserted.id
            values
            (@orderId, @token, @targetStatus, @used, @expiresAt)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@orderId", token.OrderId),
            new QueryParameter("@token", token.Token),
            new QueryParameter("@targetStatus", token.TargetStatus.ToDbValue()),
            new QueryParameter("@used", token.Used),
            new QueryParameter("@expiresAt", token.ExpiresAt)
            ],
            cancellationToken);
    }

    public async Task<bool> MarkUsedAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "update OrderStatusToken set used=1 where id=@id and used=0",
            [new QueryParameter("@id", id)],
            cancellationToken
        ) == 1;
    }
}
