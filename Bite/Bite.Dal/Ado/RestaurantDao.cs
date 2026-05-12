using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Bite.Dal.Ado
{
    public class RestaurantDao(IConnectionFactory connectionFactory)
    {

        public IEnumerable<Restaurant> FindAll()
        {
            const string sql = @"
            SELECT id, name, menu_id, address_id, webhook_url, title_image_path
            FROM Restaurant
            ORDER BY name";

            //await using var connection = new SqlConnection(connectionString);
            //await connection.OpenAsync();

            using DbConnection connection = connectionFactory.CreateConnection();

            //await using var cmd = new SqlCommand(sql, connection);
            //await using var reader = await cmd.ExecuteReaderAsync();

            using DbCommand command = connection.CreateCommand();
            command.CommandText = sql;


            
            using DbDataReader reader = command.ExecuteReader();

            var result = new List<Restaurant>();
            while (reader.Read())
                result.Add(MapRestaurant(reader));

            return result;
        }

        //public async Task<Restaurant?> FindByIdAsync(int id)
        //{
        //    const string sql = @"
        //    SELECT id, name, menu_id, address_id, webhook_url, title_image_path, created_at
        //    FROM Restaurant
        //    WHERE id = @id";

        //    await using var connection = new SqlConnection(connectionString);
        //    await connection.OpenAsync();

        //    await using var cmd = new SqlCommand(sql, connection);
        //    cmd.Parameters.AddWithValue("@id", id);

        //    await using var reader = await cmd.ExecuteReaderAsync();
        //    return reader.Read() ? MapRestaurant(reader) : null;
        //}

        //public async Task<int> InsertAsync(Restaurant restaurant)
        //{
        //    const string sql = @"
        //    INSERT INTO Restaurant (name, menu_id, address_id, webhook_url, title_image_path)
        //    OUTPUT INSERTED.id
        //    VALUES (@name, @menuId, @addressId, @webhook, @image)";

        //    await using var connection = new SqlConnection(connectionString);
        //    await connection.OpenAsync();

        //    await using var cmd = new SqlCommand(sql, connection);
        //    cmd.Parameters.AddWithValue("@name", restaurant.Name);
        //    cmd.Parameters.AddWithValue("@menuId", restaurant.MenuId);
        //    cmd.Parameters.AddWithValue("@addressId", restaurant.AddressId);
        //    cmd.Parameters.AddWithValue("@webhook", restaurant.WebhookUrl);
        //    cmd.Parameters.AddWithValue("@image", restaurant.TitleImagePath);

        //    var result = await cmd.ExecuteScalarAsync();
        //    return Convert.ToInt32(result);
        //}

        //public async Task<bool> UpdateAsync(Restaurant restaurant)
        //{
        //    const string sql = @"
        //    UPDATE Restaurant
        //    SET name             = @name,
        //        webhook_url      = @webhook,
        //        title_image_path = @image
        //    WHERE id = @id";

        //    await using var connection = new SqlConnection(connectionString);
        //    await connection.OpenAsync();

        //    await using var cmd = new SqlCommand(sql, connection);
        //    cmd.Parameters.AddWithValue("@name", restaurant.Name);
        //    cmd.Parameters.AddWithValue("@webhook", restaurant.WebhookUrl);
        //    cmd.Parameters.AddWithValue("@image", restaurant.TitleImagePath);
        //    cmd.Parameters.AddWithValue("@id", restaurant.Id);

        //    return await cmd.ExecuteNonQueryAsync() > 0;
        //}

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

        private static Restaurant MapRestaurant(DbDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            MenuId = r.GetInt32(2),
            AddressId = r.GetInt32(3),
            WebhookUrl = r.GetString(4),
            TitleImagePath = r.GetString(5)//,
            //CreatedAt = r.GetDateTime(6),
        };
    }
}
