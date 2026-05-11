using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado
{
    public class DeliveryFeeRuleDao
    {
        private readonly string connectionString;

        public DeliveryFeeRuleDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
        }

        public async Task<IEnumerable<DeliveryFeeRule>> FindByZoneIdAsync(int zoneId)
        {
            const string sql = @"
            SELECT id, delivery_zone_id, max_order_value, delivery_fee
            FROM DeliveryFeeRule
            WHERE delivery_zone_id = @zoneId
            ORDER BY max_order_value";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@zoneId", zoneId);

            await using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<DeliveryFeeRule>();
            while (reader.Read())
                result.Add(MapRule(reader));

            return result;
        }

        public async Task<int> InsertAsync(DeliveryFeeRule rule)
        {
            const string sql = @"
            INSERT INTO DeliveryFeeRule (delivery_zone_id, max_order_value, delivery_fee)
            OUTPUT INSERTED.id
            VALUES (@zoneId, @maxOrder, @fee)";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@zoneId", rule.DeliveryZoneId);
            cmd.Parameters.AddWithValue("@maxOrder", rule.MaxOrderValue);
            cmd.Parameters.AddWithValue("@fee", rule.DeliveryFee);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        
        // Löscht alle Regeln einer Zone – wird z.B. beim Ersetzen der
        // Lieferbedingungen (Anforderung 3) benötigt
        public async Task<bool> DeleteByZoneIdAsync(int zoneId)
        {
            const string sql = "DELETE FROM DeliveryFeeRule WHERE delivery_zone_id = @zoneId";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@zoneId", zoneId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static DeliveryFeeRule MapRule(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            DeliveryZoneId = r.GetInt32(1),
            MaxOrderValue = r.GetDecimal(2),
            DeliveryFee = r.GetDecimal(3),
        };
    }
}
