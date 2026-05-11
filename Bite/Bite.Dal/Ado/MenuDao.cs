using Bite.Dal.Common;
using Bite.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Dal.Ado;
    public class MenuDao
    {
        private readonly string connectionString;
        private readonly MenuItemDao menuItemDao;

        public MenuDao(DatabaseConfig config)
        {
            connectionString = config.TargetConnectionString;
            menuItemDao = new MenuItemDao(config);
        }

        public async Task<Menu?> FindByRestaurantIdAsync(int restaurantId)
        {
            const string sql = @"
            SELECT m.id
            FROM Menu m
            JOIN Restaurant r ON r.menu_id = m.id
            WHERE r.id = @restaurantId";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@restaurantId", restaurantId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read()) return null;

            int menuId = reader.GetInt32(0);
            await reader.CloseAsync();

            // Items separat laden
            var items = await menuItemDao.FindByMenuIdAsync(menuId);

            return new Menu
            {
                Id = menuId,
                Items = items.ToList()
            };
        }

        public async Task<int> InsertAsync()
        {
            const string sql = "INSERT INTO Menu DEFAULT VALUES; SELECT SCOPE_IDENTITY();";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var cmd = new SqlCommand(sql, connection);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }


        // Ersetzt alle MenuItems eines Menüs atomar (DELETE + INSERT in einer Transaktion).
        // Wird für Anforderung 4 "Speisekarte hochladen oder aktualisieren" verwendet.
        public async Task<bool> ReplaceMenuItemsAsync(int menuId, IEnumerable<MenuItem> items)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Alle bestehenden Items löschen
                await using var deleteCmd = new SqlCommand(
                    "DELETE FROM MenuItem WHERE menu_id = @menuId", connection, transaction);
                deleteCmd.Parameters.AddWithValue("@menuId", menuId);
                await deleteCmd.ExecuteNonQueryAsync();

                // 2. Neue Items einfügen
                const string insertSql = @"
                INSERT INTO MenuItem (menu_id, category_id, name, description, price)
                VALUES (@menuId, @catId, @name, @desc, @price)";

                foreach (var item in items)
                {
                    await using var insertCmd = new SqlCommand(insertSql, connection, transaction);
                    insertCmd.Parameters.AddWithValue("@menuId", menuId);
                    insertCmd.Parameters.AddWithValue("@catId", item.CategoryId);
                    insertCmd.Parameters.AddWithValue("@name", item.Name);
                    insertCmd.Parameters.AddWithValue("@desc", (object?)item.Description ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@price", item.Price);
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
    }

