using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace Bite.Dal.Ado;

public class DeliveryFeeRuleDao(IConnectionFactory connectionFactory) : IDeliveryFeeRuleDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private DeliveryFeeRule MapRowToDeliveryFeeRule(IDataRecord row)
    {
        return new DeliveryFeeRule(
            id: (int)row["id"],
            deliveryZoneId: (int)row["delivery_zone_id"],
            maxOrderValue: (decimal)row["max_order_value"],
            deliveryFee: (decimal)row["delivery_fee"]);
    }

    public async Task<IEnumerable<DeliveryFeeRule>> FindByRestaurantIdAndZoneIdAsync(int restaurantId, int zoneId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            """
            select dfr.* from DeliveryFeeRule dfr
            join DeliveryZone dz on dz.id = dfr.delivery_zone_id
            where dz.restaurant_id=@restaurantId and dfr.delivery_zone_id=@zoneId
            """,
            MapRowToDeliveryFeeRule,
            [
            new QueryParameter("@restaurantId", restaurantId),
            new QueryParameter("@zoneId", zoneId)
            ],
            cancellationToken);
    }

    public async Task<int> InsertAsync(DeliveryFeeRule deliveryFeeRule, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into DeliveryFeeRule
            (delivery_zone_id, max_order_value, delivery_fee)
            output inserted.id
            values
            (@deliveryZoneId, @maxOrderValue, @deliveryFee)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@deliveryZoneId", deliveryFeeRule.DeliveryZoneId),
            new QueryParameter("@maxOrderValue", deliveryFeeRule.MaxOrderValue),
            new QueryParameter("@deliveryFee", deliveryFeeRule.DeliveryFee)
            ],
            cancellationToken);
    }

    public async Task<int> DeleteAllByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            delete dfr from DeliveryFeeRule dfr
            join DeliveryZone dz on dz.id = dfr.delivery_zone_id
            where dz.restaurant_id=@restaurantId
            """,
            [
            new QueryParameter("@restaurantId", restaurantId)
            ],
            cancellationToken
        );
    }
}
