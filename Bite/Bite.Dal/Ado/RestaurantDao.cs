using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;

namespace Bite.Dal.Ado;

public class RestaurantDao(IConnectionFactory connectionFactory) : IRestaurantDao
{
    private readonly AdoTemplate template = new AdoTemplate(connectionFactory);

    private Restaurant MapRowToRestaurant(IDataRecord row)
    {
        return new Restaurant(
            id: (int)row["id"],
            name: (string)row["name"],
            addressId: (int)row["address_id"],
            webhookUrl: row["webhook_url"] as string,
            titleImagePath: row["title_image_path"] as string,
            createdAt: (DateTime)row["created_at"],
            updatedAt: (DateTime)row["updated_at"]);
    }

    public async Task<IEnumerable<Restaurant>> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await template.QueryAsync("select * from Restaurant", MapRowToRestaurant, [], cancellationToken);
    }

    public async Task<Restaurant?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            $"select * from Restaurant where id=@id", 
            MapRowToRestaurant, 
            [
            new QueryParameter("@id", id)
            ],
            cancellationToken
        );
    }

    public async Task<int?> InsertAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        return await template.QuerySingleAsync(
            """
            insert into Restaurant
            (name, menu_id, address_id, webhook_url, title_image_path)
            output inserted.id
            values
            (@name, @menuId, @addressId, @webhook, @image)
            """,
            row => (int)row[0],
            [
            new QueryParameter("@name", restaurant.Name),
            new QueryParameter("@addressId", restaurant.AddressId),
            new QueryParameter("@webhook", restaurant.WebhookUrl),
            new QueryParameter("@image", restaurant.TitleImagePath)
            ],
            cancellationToken
        );
    }

    public async Task<bool> UpdateAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            """
            update Restaurant
            set name=@name, menu_id=@menuId, address_id=@addressId, webhook_url=@webhook, title_image_path=@image
            where id=@id
            """,
            [
            new QueryParameter("@name", restaurant.Name),
            new QueryParameter("@addressId", restaurant.AddressId),
            new QueryParameter("@webhook", restaurant.WebhookUrl),
            new QueryParameter("@image", restaurant.TitleImagePath),
            new QueryParameter("@id", restaurant.Id)
            ],
            cancellationToken
        ) == 1;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await template.ExecuteAsync(
            "delete from Restaurant where id=@id",
            [
            new QueryParameter("@id", id)
            ],
            cancellationToken
        ) == 1;
    }

    //// FULL AI!
    //// Sucht Restaurants im Umkreis von maxDistanceKm um den gegebenen GPS-Punkt.
    //// Verwendet die Haversine-Formel direkt in SQL.
    //// onlyOpen=true filtert auf Restaurants, die zum aktuellen Zeitpunkt geöffnet haben.
    //// Ergebnisse sind nach Entfernung aufsteigend sortiert.
    //public async Task<IEnumerable<Restaurant>> FindByLocationAsync(
    //    double lat, double lon, double maxDistanceKm, bool onlyOpen)
    //{
    //    // Haversine in T-SQL:
    //    // d = 2 * R * ASIN(SQRT(
    //    //       POWER(SIN((lat2-lat1)*PI()/180/2), 2) +
    //    //       COS(lat1*PI()/180) * COS(lat2*PI()/180) *
    //    //       POWER(SIN((lon2-lon1)*PI()/180/2), 2)))
    //    // R = 6371 km
    //    string sql = @"
    //    SELECT r.id, r.name, r.menu_id, r.address_id,
    //           r.webhook_url, r.title_image_path, r.created_at,
    //           (2 * 6371 * ASIN(SQRT(
    //               POWER(SIN((a.latitude  - @lat) * PI() / 180 / 2), 2) +
    //               COS(@lat * PI() / 180) * COS(a.latitude * PI() / 180) *
    //               POWER(SIN((a.longitude - @lon) * PI() / 180 / 2), 2)
    //           ))) AS distance_km
    //    FROM Restaurant r
    //    JOIN Address a ON r.address_id = a.id
    //    WHERE (2 * 6371 * ASIN(SQRT(
    //               POWER(SIN((a.latitude  - @lat) * PI() / 180 / 2), 2) +
    //               COS(@lat * PI() / 180) * COS(a.latitude * PI() / 180) *
    //               POWER(SIN((a.longitude - @lon) * PI() / 180 / 2), 2)
    //          ))) <= @maxDist";

    //    // Optionaler Filter: nur jetzt geöffnete Restaurants
    //    if (onlyOpen)
    //    {
    //        sql += @"
    //      AND EXISTS (
    //          SELECT 1 FROM OpeningHourSlot o
    //          WHERE o.restaurant_id = r.id
    //            AND o.day_of_week   = @dayOfWeek
    //            AND o.open_time    <= @timeNow
    //            AND o.close_time   >= @timeNow
    //      )";
    //    }

    //    sql += " ORDER BY distance_km ASC";

    //    await using var connection = new SqlConnection(connectionString);
    //    await connection.OpenAsync();

    //    await using var cmd = new SqlCommand(sql, connection);
    //    cmd.Parameters.AddWithValue("@lat", lat);
    //    cmd.Parameters.AddWithValue("@lon", lon);
    //    cmd.Parameters.AddWithValue("@maxDist", maxDistanceKm);

    //    if (onlyOpen)
    //    {
    //        var now = DateTime.Now;
    //        // SQL Server DayOfWeek: Sonntag=0 ... Samstag=6 (passt zu .NET DayOfWeek)
    //        cmd.Parameters.AddWithValue("@dayOfWeek", (int)now.DayOfWeek);
    //        cmd.Parameters.AddWithValue("@timeNow", now.TimeOfDay);
    //    }

    //    await using var reader = await cmd.ExecuteReaderAsync();

    //    var result = new List<Restaurant>();
    //    while (reader.Read())
    //        result.Add(MapRestaurant(reader));

    //    return result;
    //}
}
