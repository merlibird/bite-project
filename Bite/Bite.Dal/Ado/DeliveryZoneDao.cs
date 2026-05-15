using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Bite.Dal.Ado;

public class DeliveryZoneDao(IConnectionFactory connectionFactory) : IDeliveryZoneDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private DeliveryZone MapRowToDeliveryZone(IDataRecord row)
    {
        return new DeliveryZone(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            minOrderValue: (decimal)row["min_order_value"],
            maxDistance: (double)row["max_distance"]);
    }

    public async Task<IEnumerable<DeliveryZone>> FindByRestaurantIdAsync(int restaurantId)
    {
        return await template.QueryAsync(
            "select * from DeliveryZone where restaurant_id=@restaurantId",
            MapRowToDeliveryZone,
            new QueryParameter("@restaurantId", restaurantId));
    }

    public async Task<int?> InsertAsync(DeliveryZone deliveryZone)
    {
        return await template.QuerySingleAsync(
            """
            insert into DeliveryZone
            (restaurant_id, min_order_value, max_distance)
            output inserted.id
            values
            (@restaurantId, @minOrderValue, @maxDistance)
            """,
            row => (int)row[0],
            new QueryParameter("@restaurantId", deliveryZone.RestaurantId),
            new QueryParameter("@minOrderValue", deliveryZone.MinOrderValue),
            new QueryParameter("@maxDistance", deliveryZone.MaxDistance));
    }

    public async Task<bool> UpdateAsync(DeliveryZone deliveryZone)
    {
        return await template.ExecuteAsync(
            """
            update DeliveryZone
            set min_order_value=@minOrderValue, max_distance=@maxDistance
            where id=@id
            """,
            new QueryParameter("@minOrderValue", deliveryZone.MinOrderValue),
            new QueryParameter("@maxDistance", deliveryZone.MaxDistance),
            new QueryParameter("@id", deliveryZone.Id)
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await template.ExecuteAsync(
            "delete from DeliveryZone where id=@id",
            new QueryParameter("@id", id)
        ) == 1;
    }
}
