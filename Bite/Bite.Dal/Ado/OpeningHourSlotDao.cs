using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado
{
    public class OpeningHourSlotDao
    {
        private readonly string connectionString;

        public OpeningHourSlotDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
        }

        public async Task<IEnumerable<OpeningHourSlot>> FindByRestaurantIdAsync(int restaurantId)
        {
            const string sql = @"
            SELECT id, restaurant_id, day_of_week, open_time, close_time
            FROM OpeningHourSlot
            WHERE restaurant_id = @restaurantId
            ORDER BY day_of_week, open_time";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@restaurantId", restaurantId);

            await using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<OpeningHourSlot>();
            while (reader.Read())
                result.Add(MapSlot(reader));

            return result;
        }

        /// Prüft ob das Restaurant zum jetzigen Zeitpunkt geöffnet ist.
        /// Berücksichtigt mehrere Slots pro Tag (z.B. Mittagspause).
        public async Task<bool> IsOpenNowAsync(int restaurantId)
        {
            const string sql = @"
            SELECT COUNT(*)
            FROM OpeningHourSlot
            WHERE restaurant_id = @restaurantId
              AND day_of_week   = @dayOfWeek
              AND open_time    <= @timeNow
              AND close_time   >= @timeNow";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            var now = DateTime.Now;
            cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
            cmd.Parameters.AddWithValue("@dayOfWeek", (int)now.DayOfWeek);
            cmd.Parameters.AddWithValue("@timeNow", now.TimeOfDay);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        /// Ersetzt alle Öffnungszeiten eines Restaurants atomar.
        /// Wird für Anforderung 1 (Update der Restaurantdaten) verwendet.
        public async Task<bool> ReplaceAsync(int restaurantId, IEnumerable<OpeningHourSlot> slots)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Alle bestehenden Slots löschen (ON DELETE CASCADE greift hier nicht)
                await using var deleteCmd = new SqlCommand(
                    "DELETE FROM OpeningHourSlot WHERE restaurant_id = @restaurantId",
                    connection, transaction);
                deleteCmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                await deleteCmd.ExecuteNonQueryAsync();

                // 2. Neue Slots einfügen
                const string insertSql = @"
                INSERT INTO OpeningHourSlot (restaurant_id, day_of_week, open_time, close_time)
                VALUES (@restaurantId, @day, @open, @close)";

                foreach (var slot in slots)
                {
                    await using var insertCmd = new SqlCommand(insertSql, connection, transaction);
                    insertCmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                    insertCmd.Parameters.AddWithValue("@day", slot.DayOfWeek);
                    insertCmd.Parameters.AddWithValue("@open", slot.OpenTime);
                    insertCmd.Parameters.AddWithValue("@close", slot.CloseTime);
                    await insertCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static OpeningHourSlot MapSlot(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            RestaurantId = r.GetInt32(1),
            DayOfWeek = r.GetInt32(2),
            OpenTime = r.GetTimeSpan(3),
            CloseTime = r.GetTimeSpan(4),
        };
    }
}
