//using Xunit;
//using Microsoft.Data.SqlClient;
//using Bite.Dal.Ado;
//using Bite.Domain;

//namespace Bite.Tests.UnitTests;

//[Collection("Database")]                              // <-- teilt Fixture mit DatabaseTests
//public class RestaurantDaoTests                       // kein IClassFixture<> mehr nötig
//{
//    private readonly RestaurantDao _dao;
//    private readonly string _connectionString;

//    public RestaurantDaoTests(DatabaseFixture fixture)
//    {
//        _connectionString = fixture.ConnectionString;
//        var config = fixture.DbConfig;
//        _dao = new RestaurantDao(config);
//    }

//    // -----------------------------------------------------------------------
//    // FindAllAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task FindAllAsync_ShouldReturnTwentyRestaurants()
//    {
//        var result = await _dao.FindAllAsync();

//        Assert.Equal(20, result.Count());
//    }

//    [Fact]
//    public async Task FindAllAsync_ShouldReturnRestaurantsOrderedByName()
//    {
//        var result = (await _dao.FindAllAsync()).ToList();

//        var names = result.Select(r => r.Name).ToList();
//        var sortedNames = names.OrderBy(n => n).ToList();

//        Assert.Equal(sortedNames, names);
//    }

//    [Fact]
//    public async Task FindAllAsync_ShouldReturnRestaurantsWithValidIds()
//    {
//        var result = await _dao.FindAllAsync();

//        Assert.All(result, r => Assert.True(r.Id > 0));
//    }

//    [Fact]
//    public async Task FindAllAsync_ShouldContainAllThreeNamedRestaurants()
//    {
//        var result = await _dao.FindAllAsync();
//        var names = result.Select(r => r.Name).ToList();

//        Assert.Contains("Restaurant Nimmersatt", names);
//        Assert.Contains("Burger Bude Wien", names);
//        Assert.Contains("Sakura Sushi", names);
//    }

//    [Fact]
//    public async Task FindAllAsync_ShouldReturnRestaurantsWithPositiveMenuAndAddressIds()
//    {
//        var result = await _dao.FindAllAsync();

//        Assert.All(result, r =>
//        {
//            Assert.True(r.MenuId > 0);
//            Assert.True(r.AddressId > 0);
//        });
//    }

//    [Fact]
//    public async Task FindAllAsync_ShouldReturnRestaurantsWithNonEmptyWebhookAndImage()
//    {
//        var result = await _dao.FindAllAsync();

//        Assert.All(result, r =>
//        {
//            Assert.False(string.IsNullOrWhiteSpace(r.WebhookUrl));
//            Assert.False(string.IsNullOrWhiteSpace(r.TitleImagePath));
//        });
//    }

//    // -----------------------------------------------------------------------
//    // FindByIdAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task FindByIdAsync_WithValidId_ShouldReturnRestaurant()
//    {
//        int existingId = GetFirstRestaurantId();

//        var result = await _dao.FindByIdAsync(existingId);

//        Assert.NotNull(result);
//        Assert.Equal(existingId, result.Id);
//    }

//    [Fact]
//    public async Task FindByIdAsync_WithInvalidId_ShouldReturnNull()
//    {
//        var result = await _dao.FindByIdAsync(int.MaxValue);

//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task FindByIdAsync_WithNegativeId_ShouldReturnNull()
//    {
//        var result = await _dao.FindByIdAsync(-1);

//        Assert.Null(result);
//    }

//    [Theory]
//    [InlineData("Restaurant Nimmersatt")]
//    [InlineData("Burger Bude Wien")]
//    [InlineData("Sakura Sushi")]
//    public async Task FindByIdAsync_NamedRestaurant_ShouldReturnCorrectData(string name)
//    {
//        int id = GetRestaurantIdByName(name);

//        var result = await _dao.FindByIdAsync(id);

//        Assert.NotNull(result);
//        Assert.Equal(name, result.Name);
//        Assert.True(result.MenuId > 0);
//        Assert.True(result.AddressId > 0);
//        Assert.False(string.IsNullOrWhiteSpace(result.WebhookUrl));
//        Assert.False(string.IsNullOrWhiteSpace(result.TitleImagePath));
//        Assert.True(result.CreatedAt > DateTime.MinValue);
//    }

//    [Fact]
//    public async Task FindByIdAsync_ShouldReturnSameDataAsInDatabase()
//    {
//        int id = GetRestaurantIdByName("Restaurant Nimmersatt");

//        var result = await _dao.FindByIdAsync(id);

//        Assert.NotNull(result);
//        Assert.Equal("https://api.nimmersatt.at/bite", result.WebhookUrl);
//        Assert.Equal("img/nimmersatt.png", result.TitleImagePath);
//    }

//    // -----------------------------------------------------------------------
//    // InsertAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task InsertAsync_ShouldReturnPositiveId()
//    {
//        var (menuId, addressId) = CreateMenuAndAddress();
//        var restaurant = BuildTestRestaurant(menuId, addressId, "Insert Test Restaurant");

//        int newId = await _dao.InsertAsync(restaurant);

//        Assert.True(newId > 0);

//        CleanupRestaurant(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_ShouldPersistRestaurantInDatabase()
//    {
//        var (menuId, addressId) = CreateMenuAndAddress();
//        var restaurant = BuildTestRestaurant(menuId, addressId, "Persist Test Restaurant");

//        int newId = await _dao.InsertAsync(restaurant);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Equal("Persist Test Restaurant", fetched.Name);
//        Assert.Equal(menuId, fetched.MenuId);
//        Assert.Equal(addressId, fetched.AddressId);
//        Assert.Equal(restaurant.WebhookUrl, fetched.WebhookUrl);
//        Assert.Equal(restaurant.TitleImagePath, fetched.TitleImagePath);

//        CleanupRestaurant(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_TwoRestaurants_ShouldReturnDistinctIds()
//    {
//        var (menuId1, addressId1) = CreateMenuAndAddress();
//        var (menuId2, addressId2) = CreateMenuAndAddress();

//        int id1 = await _dao.InsertAsync(BuildTestRestaurant(menuId1, addressId1, "Distinct ID Test 1"));
//        int id2 = await _dao.InsertAsync(BuildTestRestaurant(menuId2, addressId2, "Distinct ID Test 2"));

//        Assert.NotEqual(id1, id2);

//        CleanupRestaurant(id1);
//        CleanupRestaurant(id2);
//    }

//    [Fact]
//    public async Task InsertAsync_ShouldIncreaseTotalCountByOne()
//    {
//        int countBefore = GetCount("Restaurant");
//        var (menuId, addressId) = CreateMenuAndAddress();

//        int newId = await _dao.InsertAsync(BuildTestRestaurant(menuId, addressId, "Count Test Restaurant"));

//        int countAfter = GetCount("Restaurant");
//        Assert.Equal(countBefore + 1, countAfter);

//        CleanupRestaurant(newId);
//    }

//    // -----------------------------------------------------------------------
//    // UpdateAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task UpdateAsync_WithValidRestaurant_ShouldReturnTrue()
//    {
//        int id = GetRestaurantIdByName("Sakura Sushi");
//        var restaurant = await _dao.FindByIdAsync(id);
//        restaurant!.Name = "Sakura Sushi Updated";

//        bool result = await _dao.UpdateAsync(restaurant);

//        Assert.True(result);

//        // Zurücksetzen
//        restaurant.Name = "Sakura Sushi";
//        await _dao.UpdateAsync(restaurant);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldPersistChangesInDatabase()
//    {
//        int id = GetRestaurantIdByName("Burger Bude Wien");
//        var restaurant = await _dao.FindByIdAsync(id);
//        string originalWebhook = restaurant!.WebhookUrl;
//        string originalImage = restaurant.TitleImagePath;

//        restaurant.WebhookUrl = "https://updated.webhook.test/orders";
//        restaurant.TitleImagePath = "img/updated_burger.png";
//        await _dao.UpdateAsync(restaurant);

//        var fetched = await _dao.FindByIdAsync(id);
//        Assert.Equal("https://updated.webhook.test/orders", fetched!.WebhookUrl);
//        Assert.Equal("img/updated_burger.png", fetched.TitleImagePath);

//        // Zurücksetzen
//        restaurant.WebhookUrl = originalWebhook;
//        restaurant.TitleImagePath = originalImage;
//        await _dao.UpdateAsync(restaurant);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldNotChangeMenuIdOrAddressId()
//    {
//        int id = GetRestaurantIdByName("Restaurant Nimmersatt");
//        var restaurant = await _dao.FindByIdAsync(id);
//        int originalMenuId = restaurant!.MenuId;
//        int originalAddressId = restaurant.AddressId;

//        restaurant.Name = "Nimmersatt Renamed";
//        await _dao.UpdateAsync(restaurant);

//        var fetched = await _dao.FindByIdAsync(id);
//        Assert.Equal(originalMenuId, fetched!.MenuId);
//        Assert.Equal(originalAddressId, fetched.AddressId);

//        // Zurücksetzen
//        restaurant.Name = "Restaurant Nimmersatt";
//        await _dao.UpdateAsync(restaurant);
//    }

//    [Fact]
//    public async Task UpdateAsync_WithNonExistentId_ShouldReturnFalse()
//    {
//        var ghost = new Restaurant
//        {
//            Id = int.MaxValue,
//            Name = "Ghost Restaurant",
//            WebhookUrl = "https://ghost.webhook.test",
//            TitleImagePath = "img/ghost.png"
//        };

//        bool result = await _dao.UpdateAsync(ghost);

//        Assert.False(result);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldOnlyUpdateNameWebhookAndImage()
//    {
//        var (menuId, addressId) = CreateMenuAndAddress();
//        int newId = await _dao.InsertAsync(
//            BuildTestRestaurant(menuId, addressId, "Update Field Test"));

//        var restaurant = await _dao.FindByIdAsync(newId);
//        restaurant!.Name = "Update Field Test – Renamed";
//        restaurant.WebhookUrl = "https://new.hook.test";
//        restaurant.TitleImagePath = "img/new.png";
//        // MenuId und AddressId bewusst unverändert

//        await _dao.UpdateAsync(restaurant);
//        var after = await _dao.FindByIdAsync(newId);

//        Assert.Equal("Update Field Test – Renamed", after!.Name);
//        Assert.Equal("https://new.hook.test", after.WebhookUrl);
//        Assert.Equal("img/new.png", after.TitleImagePath);
//        Assert.Equal(menuId, after.MenuId);
//        Assert.Equal(addressId, after.AddressId);

//        CleanupRestaurant(newId);
//    }

//    // -----------------------------------------------------------------------
//    // FindByLocationAsync
//    // -----------------------------------------------------------------------

//    // Nimmersatt: lat=48.3684, lon=14.5144 (Hagenberg)
//    // Sakura Sushi: lat=48.1970, lon=16.3540 (Wien Mariahilf)
//    // Burger Bude Wien: Wien 1010 Hauptstraße

//    [Fact]
//    public async Task FindByLocationAsync_ExactLocation_ShouldFindNimmersatt()
//    {
//        // Direkt am Standort des Restaurants suchen (Radius 1 km)
//        var result = await _dao.FindByLocationAsync(48.3684, 14.5144, 1.0, onlyOpen: false);

//        Assert.Contains(result, r => r.Name == "Restaurant Nimmersatt");
//    }

//    [Fact]
//    public async Task FindByLocationAsync_VerySmallRadius_ShouldExcludeDistantRestaurants()
//    {
//        // 0,5 km Radius am Nimmersatt-Standort – Sakura Sushi (>200 km entfernt) darf nicht auftauchen
//        var result = await _dao.FindByLocationAsync(48.3684, 14.5144, 0.5, onlyOpen: false);

//        Assert.DoesNotContain(result, r => r.Name == "Sakura Sushi");
//    }

//    [Fact]
//    public async Task FindByLocationAsync_LargeRadius_ShouldReturnAllRestaurants()
//    {
//        // 5000 km – alle 20 Restaurants müssen gefunden werden
//        var result = await _dao.FindByLocationAsync(48.2082, 16.3738, 5000.0, onlyOpen: false);

//        Assert.Equal(20, result.Count());
//    }

//    [Fact]
//    public async Task FindByLocationAsync_ShouldReturnResultsOrderedByDistance()
//    {
//        // Vom Mittelpunkt Wiens suchen: Sakura (Wien) muss vor Nimmersatt (Hagenberg) kommen
//        var result = (await _dao.FindByLocationAsync(48.2082, 16.3738, 5000.0, onlyOpen: false))
//                     .ToList();

//        int sakuraIndex = result.FindIndex(r => r.Name == "Sakura Sushi");
//        int nimmersattIndex = result.FindIndex(r => r.Name == "Restaurant Nimmersatt");

//        Assert.True(sakuraIndex < nimmersattIndex,
//            "Sakura Sushi (Wien) sollte vor Restaurant Nimmersatt (Hagenberg) erscheinen.");
//    }

//    [Fact]
//    public async Task FindByLocationAsync_ZeroRadius_ShouldReturnNoResults()
//    {
//        // Radius 0 – kein Restaurant kann exakt auf dem Suchpunkt liegen
//        var result = await _dao.FindByLocationAsync(0.0, 0.0, 0.0, onlyOpen: false);

//        Assert.Empty(result);
//    }

//    [Fact]
//    public async Task FindByLocationAsync_OnlyOpen_ShouldReturnSubsetOfAllResults()
//    {
//        var allResults = (await _dao.FindByLocationAsync(48.2082, 16.3738, 5000.0, onlyOpen: false))
//                         .Select(r => r.Id).ToHashSet();

//        var openResults = (await _dao.FindByLocationAsync(48.2082, 16.3738, 5000.0, onlyOpen: true))
//                          .Select(r => r.Id).ToList();

//        // Jedes „jetzt geöffnet"-Restaurant muss auch in der ungefilterten Liste sein
//        Assert.All(openResults, id => Assert.Contains(id, allResults));
//        Assert.True(openResults.Count <= allResults.Count);
//    }

//    [Fact]
//    public async Task FindByLocationAsync_OnlyOpen_ShouldReturnRestaurantsWithActiveSlot()
//    {
//        var openResults = await _dao.FindByLocationAsync(48.2082, 16.3738, 5000.0, onlyOpen: true);

//        var now = DateTime.Now;

//        foreach (var restaurant in openResults)
//        {
//            bool hasActiveSlot = HasActiveOpeningSlot(restaurant.Id, now);
//            Assert.True(hasActiveSlot,
//                $"Restaurant '{restaurant.Name}' (Id={restaurant.Id}) wurde als geöffnet zurückgegeben, " +
//                $"hat aber keinen aktiven Slot für {now:ddd HH:mm}.");
//        }
//    }

//    [Fact]
//    public async Task FindByLocationAsync_NearSakura_ShouldFindSakuraFirst()
//    {
//        // Direkt am Sakura-Standort suchen
//        var result = (await _dao.FindByLocationAsync(48.1970, 16.3540, 500.0, onlyOpen: false))
//                     .ToList();

//        Assert.NotEmpty(result);
//        Assert.Equal("Sakura Sushi", result.First().Name);
//    }

//    [Fact]
//    public async Task FindByLocationAsync_ShouldReturnValidRestaurantObjects()
//    {
//        var result = await _dao.FindByLocationAsync(48.3684, 14.5144, 200.0, onlyOpen: false);

//        Assert.All(result, r =>
//        {
//            Assert.True(r.Id > 0);
//            Assert.False(string.IsNullOrWhiteSpace(r.Name));
//            Assert.True(r.MenuId > 0);
//            Assert.True(r.AddressId > 0);
//        });
//    }

//    // -----------------------------------------------------------------------
//    // Hilfsmethoden
//    // -----------------------------------------------------------------------

//    private int GetFirstRestaurantId()
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand("SELECT TOP 1 id FROM Restaurant ORDER BY id", conn);
//        return (int)cmd.ExecuteScalar();
//    }

//    private int GetRestaurantIdByName(string name)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand("SELECT id FROM Restaurant WHERE name = @name", conn);
//        cmd.Parameters.AddWithValue("@name", name);
//        var result = cmd.ExecuteScalar()
//            ?? throw new InvalidOperationException($"Restaurant '{name}' nicht in der Datenbank gefunden.");
//        return (int)result;
//    }

//    private int GetCount(string tableName)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}]", conn);
//        return (int)cmd.ExecuteScalar();
//    }

//    /// <summary>
//    /// Legt ein frisches Menu + eine frische Adresse an und gibt deren IDs zurück.
//    /// Wird für Insert-Tests benötigt, um FK-Constraints zu erfüllen.
//    /// </summary>
//    private (int menuId, int addressId) CreateMenuAndAddress()
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();

//        using var menuCmd = new SqlCommand(
//            "INSERT INTO Menu DEFAULT VALUES; SELECT SCOPE_IDENTITY();", conn);
//        int menuId = Convert.ToInt32(menuCmd.ExecuteScalar());

//        using var addrCmd = new SqlCommand(@"
//            INSERT INTO Address (street, number, zip_code, city, country, longitude, latitude)
//            OUTPUT INSERTED.id
//            VALUES ('Testgasse', '1', '0000', 'Teststadt', 'Austria', 14.0, 48.0)", conn);
//        int addressId = (int)addrCmd.ExecuteScalar();

//        return (menuId, addressId);
//    }

//    private static Restaurant BuildTestRestaurant(int menuId, int addressId, string name) => new()
//    {
//        Name = name,
//        MenuId = menuId,
//        AddressId = addressId,
//        WebhookUrl = "https://test.webhook.bite/hook",
//        TitleImagePath = "img/test_restaurant.png",
//    };

//    private void CleanupRestaurant(int id)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand("DELETE FROM Restaurant WHERE id = @id", conn);
//        cmd.Parameters.AddWithValue("@id", id);
//        cmd.ExecuteNonQuery();
//    }

//    /// <summary>
//    /// Prüft direkt in der DB, ob für ein Restaurant zum angegebenen Zeitpunkt ein aktiver Slot existiert.
//    /// Wird für den OnlyOpen-Test verwendet, um das DAO-Ergebnis zu verifizieren.
//    /// </summary>
//    private bool HasActiveOpeningSlot(int restaurantId, DateTime at)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand(@"
//            SELECT COUNT(*)
//            FROM OpeningHourSlot
//            WHERE restaurant_id = @id
//              AND day_of_week   = @day
//              AND open_time    <= @time
//              AND close_time   >= @time", conn);
//        cmd.Parameters.AddWithValue("@id", restaurantId);
//        cmd.Parameters.AddWithValue("@day", (int)at.DayOfWeek);
//        cmd.Parameters.AddWithValue("@time", at.TimeOfDay);
//        return (int)cmd.ExecuteScalar() > 0;
//    }
//}