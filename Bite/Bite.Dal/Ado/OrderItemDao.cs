using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class OrderItemDao(IConnectionFactory connectionFactory) : IOrderItemDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private OrderItem MapRowToOrderItem(IDataRecord row)
    {
        return new OrderItem(
            id: (int)row["id"],
            orderId: (int)row["order_id"],
            menuItemId: (int)row["menu_item_id"],
            quantity: (int)row["quantity"],
            unitPrice: (decimal)row["unit_price"]);
    }

    public async Task<IEnumerable<OrderItem>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            "select * from OrderItem where order_id=@orderId",
            MapRowToOrderItem,
            [new QueryParameter("@orderId", orderId)],
            cancellationToken);
    }

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
