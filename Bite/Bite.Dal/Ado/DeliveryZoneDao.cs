using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado
{
    public class DeliveryZoneDao
    {
        private readonly string connectionString;
        private readonly DeliveryFeeRuleDao feeRuleDao;

        public DeliveryZoneDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
            feeRuleDao = new DeliveryFeeRuleDao(config);
        }

        public async Task<IEnumerable<DeliveryZone>> FindByRestaurantIdAsync(int restaurantId)
        {
            const string sql = @"
            SELECT id, restaurant_id, min_order_value, max_distance
            FROM DeliveryZone
            WHERE restaurant_id = @restaurantId
            ORDER BY max_distance";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@restaurantId", restaurantId);

            await using var reader = await cmd.ExecuteReaderAsync();

            var zones = new List<DeliveryZone>();
            while (reader.Read())
                zones.Add(MapZone(reader));

            await reader.CloseAsync();

            // FeeRules für jede Zone nachladen
            foreach (var zone in zones)
                zone.FeeRules = (await feeRuleDao.FindByZoneIdAsync(zone.Id)).ToList();

            return zones;
        }

        public async Task<DeliveryZone?> FindByIdAsync(int id)
        {
            const string sql = @"
            SELECT id, restaurant_id, min_order_value, max_distance
            FROM DeliveryZone
            WHERE id = @id";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read()) return null;

            var zone = MapZone(reader);
            await reader.CloseAsync();

            zone.FeeRules = [.. (await feeRuleDao.FindByZoneIdAsync(zone.Id))]; //lt. Intellisense, hab ich auch nicht kennt
            return zone;
        }

        public async Task<int> InsertAsync(DeliveryZone zone)
        {
            const string sql = @"
            INSERT INTO DeliveryZone (restaurant_id, min_order_value, max_distance)
            OUTPUT INSERTED.id
            VALUES (@restaurantId, @minOrder, @maxDist)";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@restaurantId", zone.RestaurantId);
            cmd.Parameters.AddWithValue("@minOrder", zone.MinOrderValue);
            cmd.Parameters.AddWithValue("@maxDist", zone.MaxDistance);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Löscht alle Lieferzonen eines Restaurants inkl. zugehöriger FeeRules
        // (ON DELETE CASCADE übernimmt das Löschen der FeeRules automatisch).
        public async Task<bool> DeleteByRestaurantIdAsync(int restaurantId)
        {
            const string sql = "DELETE FROM DeliveryZone WHERE restaurant_id = @restaurantId";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@restaurantId", restaurantId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static DeliveryZone MapZone(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            RestaurantId = r.GetInt32(1),
            MinOrderValue = r.GetDecimal(2),
            MaxDistance = r.GetDouble(3),
        };
    }
}
