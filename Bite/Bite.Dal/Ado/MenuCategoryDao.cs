using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado;
    public class MenuCategoryDao
    {
        private readonly string connectionString;

        public MenuCategoryDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
        }

        public async Task<IEnumerable<MenuCategory>> FindAllAsync()
        {
            const string sql = "SELECT id, name FROM MenuCategory ORDER BY name";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();

            var result = new List<MenuCategory>();
            while (reader.Read())
                result.Add(MapCategory(reader));

            return result;
        }

        public async Task<MenuCategory?> FindByIdAsync(int id)
        {
            const string sql = "SELECT id, name FROM MenuCategory WHERE id = @id";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            return reader.Read() ? MapCategory(reader) : null;
        }

        public async Task<int> InsertAsync(string name)
        {
            // Gibt vorhandene ID zurück falls Name bereits existiert (UNIQUE constraint)
            const string sql = @"
            IF NOT EXISTS (SELECT 1 FROM MenuCategory WHERE name = @name)
                INSERT INTO MenuCategory (name) VALUES (@name);
            SELECT id FROM MenuCategory WHERE name = @name";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@name", name);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static MenuCategory MapCategory(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
        };
    }
