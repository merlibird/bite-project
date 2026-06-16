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

public class OpeningHourSlotDao(IConnectionFactory connectionFactory) : IOpeningHourSlotDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private OpeningHourSlot MapRowToOpeningHourSlot(IDataRecord row)
    {
        return new OpeningHourSlot(
            id: (int)row["id"],
            restaurantId: (int)row["restaurant_id"],
            dayOfWeek: (int)row["day_of_week"],
            openTime: (TimeSpan)row["open_time"],
            closeTime: (TimeSpan)row["close_time"]);
    }

    public async Task<IEnumerable<OpeningHourSlot>> FindByRestaurantIdAsync(int restaurantId, CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync(
            "select * from OpeningHourSlot where restaurant_id=@restaurantId",
            MapRowToOpeningHourSlot,
            [new QueryParameter("@restaurantId", restaurantId)],
            cancellationToken);
    }

    public async Task<int> InsertAsync(OpeningHourSlot openingHourSlot, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into OpeningHourSlot
            (restaurant_id, day_of_week, open_time, close_time)
            output inserted.id
            values
            (@restaurantId, @dayOfWeek, @openTime, @closeTime)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@restaurantId", openingHourSlot.RestaurantId),
            new QueryParameter("@dayOfWeek", openingHourSlot.DayOfWeek),
            new QueryParameter("@openTime", openingHourSlot.OpenTime),
            new QueryParameter("@closeTime", openingHourSlot.CloseTime)
            ],
            cancellationToken);
    }

}
