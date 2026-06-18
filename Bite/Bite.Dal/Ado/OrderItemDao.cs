using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class OrderItemDao(IConnectionFactory connectionFactory) : IOrderItemDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    public async Task<int> InsertAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into OrderItem
            (order_id, menu_item_id, quantity, unit_price)
            output inserted.id
            values
            (@orderId, @menuItemId, @quantity, @unitPrice)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@orderId", orderItem.OrderId),
            new QueryParameter("@menuItemId", orderItem.MenuItemId),
            new QueryParameter("@quantity", orderItem.Quantity),
            new QueryParameter("@unitPrice", orderItem.UnitPrice)
            ],
            cancellationToken);
    }
}
