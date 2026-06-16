using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System.Data;

namespace Bite.Dal.Ado;

public class CustomerOrderDao(IConnectionFactory connectionFactory) : ICustomerOrderDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private CustomerOrder MapRowToCustomerOrder(IDataRecord row)
    {
        return new CustomerOrder(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            addressId: (int)row["address_id"],
            orderCode: (string)row["order_code"],
            status: OrderStatusExtensions.FromDbValue((string)row["status"]),
            deliveryFee: (decimal)row["delivery_fee"],
            total: (decimal)row["total"],
            createdAt: (DateTime)row["created_at"],
            updatedAt: (DateTime)row["updated_at"]);
    }

    public async Task<CustomerOrder?> FindByOrderCodeAsync(string orderCode, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            "select * from CustomerOrder where order_code=@orderCode",
            MapRowToCustomerOrder,
            [new QueryParameter("@orderCode", orderCode)],
            cancellationToken);
    }

    public async Task<int> InsertAsync(CustomerOrder order, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into CustomerOrder
            (restaurant_id, address_id, order_code, status, delivery_fee, total)
            output inserted.id
            values
            (@restaurantId, @addressId, @orderCode, @status, @deliveryFee, @total)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@restaurantId", order.RestaurantId),
            new QueryParameter("@addressId", order.AddressId),
            new QueryParameter("@orderCode", order.OrderCode),
            new QueryParameter("@status", order.Status.ToDbValue()),
            new QueryParameter("@deliveryFee", order.DeliveryFee),
            new QueryParameter("@total", order.Total)
            ],
            cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "update CustomerOrder set status=@status where id=@id",
            [
            new QueryParameter("@status", status.ToDbValue()),
            new QueryParameter("@id", id)
            ],
            cancellationToken
        ) == 1;
    }

    public async Task<bool> UpdateStatusIfAsync(int id, OrderStatus expectedCurrentStatus, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "update CustomerOrder set status=@newStatus where id=@id and status=@expectedStatus",
            [
            new QueryParameter("@newStatus", newStatus.ToDbValue()),
            new QueryParameter("@expectedStatus", expectedCurrentStatus.ToDbValue()),
            new QueryParameter("@id", id)
            ],
            cancellationToken
        ) == 1;
    }
}
