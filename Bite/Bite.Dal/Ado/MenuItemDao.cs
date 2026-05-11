using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado;
    public class MenuItemDao
    {
        private readonly string connectionString;

        public MenuItemDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
        }

        public async Task<IEnumerable<MenuItem>> FindByMenuIdAsync(int menuId)
        {
            const string sql = @"
            SELECT id, menu_id, category_id, name, description, price, created_at
            FROM MenuItem
            WHERE menu_id = @menuId
            ORDER BY category_id, name";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@menuId", menuId);

            await using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<MenuItem>();
            while (reader.Read())
                result.Add(MapItem(reader));

            return result;
        }

        public async Task<MenuItem?> FindByIdAsync(int id)
        {
            const string sql = @"
            SELECT id, menu_id, category_id, name, description, price, created_at
            FROM MenuItem
            WHERE id = @id";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            return reader.Read() ? MapItem(reader) : null;
        }

        public async Task<int> InsertAsync(MenuItem item)
        {
            const string sql = @"
            INSERT INTO MenuItem (menu_id, category_id, name, description, price)
            OUTPUT INSERTED.id
            VALUES (@menuId, @catId, @name, @desc, @price)";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@menuId", item.MenuId);
            cmd.Parameters.AddWithValue("@catId", item.CategoryId);
            cmd.Parameters.AddWithValue("@name", item.Name);
            cmd.Parameters.AddWithValue("@desc", (object?)item.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", item.Price);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> DeleteByMenuIdAsync(int menuId)
        {
            const string sql = "DELETE FROM MenuItem WHERE menu_id = @menuId";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@menuId", menuId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static MenuItem MapItem(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            MenuId = r.GetInt32(1),
            CategoryId = r.GetInt32(2),
            Name = r.GetString(3),
            Description = r.IsDBNull(4) ? null : r.GetString(4),
            Price = r.GetDecimal(5),
            CreatedAt = r.GetDateTime(6),
        };
    }