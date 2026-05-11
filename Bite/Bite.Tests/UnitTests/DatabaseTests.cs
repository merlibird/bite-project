using Xunit;
using Microsoft.Data.SqlClient;
using Bite.Dal.Ado;
using System.Data;

namespace Bite.Tests.UnitTests;

[Collection("Database")]                              // <-- teilt Fixture mit anderen Tests
public class DatabaseTests                            
{
    private string connectionString;

    public DatabaseTests(DatabaseFixture fixture)
    {
        connectionString = fixture.ConnectionString;
    }

    // -----------------------------------------------------------------------
    // Datenbank-Basis
    // -----------------------------------------------------------------------

    [Fact]
    public void Database_ShouldBeAccessible()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    // -----------------------------------------------------------------------
    // Zählungen
    // -----------------------------------------------------------------------

    [Fact]
    public void AllTables_ShouldContainTwentyEntriesAfterFilling()
    {
        Assert.Equal(20, GetCount("Restaurant"));
        Assert.Equal(20, GetCount("Address"));
        Assert.Equal(20, GetCount("Menu"));
    }

    [Fact]
    public void MenuCategories_ShouldContainEightEntries()
    {
        Assert.Equal(8, GetCount("MenuCategory"));
    }

    [Fact]
    public void MenuItems_ShouldExist()
    {
        Assert.True(GetCount("MenuItem") > 0, "Es sollten MenuItems vorhanden sein.");
    }

    [Fact]
    public void OpeningHourSlots_ShouldExist()
    {
        Assert.True(GetCount("OpeningHourSlot") > 0, "Es sollten Öffnungszeiten vorhanden sein.");
    }

    [Fact]
    public void DeliveryZones_ShouldExist()
    {
        Assert.True(GetCount("DeliveryZone") > 0, "Es sollten Lieferzonen vorhanden sein.");
    }

    [Fact]
    public void DeliveryFeeRules_ShouldExist()
    {
        Assert.True(GetCount("DeliveryFeeRule") > 0, "Es sollten Lieferkostenregeln vorhanden sein.");
    }

    // -----------------------------------------------------------------------
    // Restaurant - Address JOIN
    // -----------------------------------------------------------------------

    [Fact]
    public void DataConsistency_AllRestaurants_ShouldHaveAddress()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*) 
            FROM Restaurant r
            JOIN Address a ON r.address_id = a.id";

        using var cmd = new SqlCommand(sql, connection);
        int joinCount = (int)cmd.ExecuteScalar();

        Assert.Equal(20, joinCount);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(20)]
    public void DataConsistency_GenericRestaurants_ShouldHaveMatchingAddress(int index)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT r.name, a.street 
            FROM Restaurant r
            JOIN Address a ON r.address_id = a.id
            WHERE r.name = @name";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", $"Bite Palace {index}");

        using var reader = cmd.ExecuteReader();

        Assert.True(reader.Read(), $"Kein Datensatz für 'Bite Palace {index}' gefunden.");
        Assert.Contains(index.ToString(), reader.GetString(1));
    }

    // -----------------------------------------------------------------------
    // Die drei realistischen Restaurants
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Restaurant Nimmersatt", "Softwarepark", "4232")]
    [InlineData("Burger Bude Wien", "Hauptstraße", "1010")]
    [InlineData("Sakura Sushi", "Mariahilfer Straße", "1060")]
    public void NamedRestaurants_ShouldHaveCorrectAddress(
        string restaurantName, string expectedStreet, string expectedZip)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT a.street, a.zip_code
            FROM Restaurant r
            JOIN Address a ON r.address_id = a.id
            WHERE r.name = @name";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", restaurantName);

        using var reader = cmd.ExecuteReader();

        Assert.True(reader.Read(), $"Restaurant '{restaurantName}' nicht gefunden.");
        Assert.Equal(expectedStreet, reader.GetString(0));
        Assert.Equal(expectedZip, reader.GetString(1));
    }

    // -----------------------------------------------------------------------
    // MenuItems
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Restaurant Nimmersatt", "Margherita")]
    [InlineData("Restaurant Nimmersatt", "Lasagne al Forno")]
    [InlineData("Burger Bude Wien", "Classic Burger")]
    [InlineData("Burger Bude Wien", "Caesar Salad")]
    [InlineData("Sakura Sushi", "California Roll (8 St.)")]
    [InlineData("Sakura Sushi", "Mochi Eis")]
    public void MenuItems_ShouldExistForRestaurant(string restaurantName, string itemName)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM MenuItem mi
            JOIN Menu m      ON mi.menu_id = m.id
            JOIN Restaurant r ON r.menu_id  = m.id
            WHERE r.name   = @restaurant
              AND mi.name  = @item";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@restaurant", restaurantName);
        cmd.Parameters.AddWithValue("@item", itemName);

        int count = (int)cmd.ExecuteScalar();
        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public void MenuItems_PricesShouldBePositive(string restaurantName)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM MenuItem mi
            JOIN Menu m       ON mi.menu_id = m.id
            JOIN Restaurant r ON r.menu_id  = m.id
            WHERE r.name = @restaurant
              AND mi.price <= 0";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@restaurant", restaurantName);

        int badPrices = (int)cmd.ExecuteScalar();
        Assert.Equal(0, badPrices);
    }

    // -----------------------------------------------------------------------
    // Öffnungszeiten
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Restaurant Nimmersatt", 2)]
    [InlineData("Burger Bude Wien", 0)]
    [InlineData("Sakura Sushi", 6)]
    public void OpeningHours_ShouldExistForDay(string restaurantName, int dayOfWeek)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM OpeningHourSlot o
            JOIN Restaurant r ON o.restaurant_id = r.id
            WHERE r.name        = @restaurant
              AND o.day_of_week = @day";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@restaurant", restaurantName);
        cmd.Parameters.AddWithValue("@day", dayOfWeek);

        Assert.True((int)cmd.ExecuteScalar() > 0,
            $"Keine Öffnungszeiten für {restaurantName} am Tag {dayOfWeek} gefunden.");
    }

    [Fact]
    public void OpeningHours_DayOfWeek_ShouldBeInValidRange()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT COUNT(*) FROM OpeningHourSlot WHERE day_of_week < 0 OR day_of_week > 6";
        using var cmd = new SqlCommand(sql, connection);

        Assert.Equal(0, (int)cmd.ExecuteScalar());
    }

    // -----------------------------------------------------------------------
    // Lieferzonen & Regeln
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Restaurant Nimmersatt")]
    [InlineData("Burger Bude Wien")]
    [InlineData("Sakura Sushi")]
    public void DeliveryZones_ShouldExistForNamedRestaurants(string restaurantName)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM DeliveryZone dz
            JOIN Restaurant r ON dz.restaurant_id = r.id
            WHERE r.name = @restaurant";

        using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@restaurant", restaurantName);

        Assert.True((int)cmd.ExecuteScalar() > 0,
            $"Keine Lieferzonen für {restaurantName} gefunden.");
    }

    [Fact]
    public void DeliveryFeeRules_ShouldAllBeNonNegative()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT COUNT(*) FROM DeliveryFeeRule WHERE delivery_fee < 0 OR max_order_value < 0";
        using var cmd = new SqlCommand(sql, connection);

        Assert.Equal(0, (int)cmd.ExecuteScalar());
    }

    [Fact]
    public void DeliveryZones_ShouldAllHaveAtLeastOneRule()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM DeliveryZone dz
            WHERE NOT EXISTS (
                SELECT 1 FROM DeliveryFeeRule dfr
                WHERE dfr.delivery_zone_id = dz.id
            )";

        using var cmd = new SqlCommand(sql, connection);
        Assert.Equal(0, (int)cmd.ExecuteScalar());
    }

    // -----------------------------------------------------------------------
    // GPS-Koordinaten
    // -----------------------------------------------------------------------

    [Fact]
    public void Addresses_LongitudeShouldBeInValidRange()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT COUNT(*) FROM Address WHERE longitude < -180 OR longitude > 180";
        using var cmd = new SqlCommand(sql, connection);
        Assert.Equal(0, (int)cmd.ExecuteScalar());
    }

    [Fact]
    public void Addresses_LatitudeShouldBeInValidRange()
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT COUNT(*) FROM Address WHERE latitude < -90 OR latitude > 90";
        using var cmd = new SqlCommand(sql, connection);
        Assert.Equal(0, (int)cmd.ExecuteScalar());
    }

    // -----------------------------------------------------------------------
    // Hilfsmethoden
    // -----------------------------------------------------------------------

    private int GetCount(string tableName)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}]", connection);
        return (int)command.ExecuteScalar();
    }
}