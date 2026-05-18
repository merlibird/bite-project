//using Bite.Dal.Ado;
//using Bite.Domain;
//using Microsoft.Data.SqlClient;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Bite.Tests.UnitTests;

//[Collection("Database")]
//public class AddressDaoTests
//{
//    private readonly AddressDao _dao;
//    private readonly string _connectionString;

//    public AddressDaoTests(DatabaseFixture fixture)
//    {
//        _connectionString = fixture.ConnectionString;
//        _dao = new AddressDao(fixture.DbConfig);
//    }

//    // -----------------------------------------------------------------------
//    // FindByIdAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task FindByIdAsync_WithValidId_ShouldReturnAddress()
//    {
//        int existingId = GetFirstAddressId();

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
//    [InlineData("Restaurant Nimmersatt", "Softwarepark", "11", "4232", "Hagenberg", "Austria", 14.5144, 48.3684)]
//    [InlineData("Sakura Sushi", "Mariahilfer Straße", "88", "1060", "Wien", "Austria", 16.3540, 48.1970)]
//    public async Task FindByIdAsync_NamedRestaurantAddress_ShouldReturnCorrectFields(
//        string restaurantName,
//        string expectedStreet, string expectedNumber, string expectedZip,
//        string expectedCity, string expectedCountry,
//        double expectedLon, double expectedLat)
//    {
//        int addressId = GetAddressIdByRestaurant(restaurantName);

//        var result = await _dao.FindByIdAsync(addressId);

//        Assert.NotNull(result);
//        Assert.Equal(expectedStreet, result.Street);
//        Assert.Equal(expectedNumber, result.Number);
//        Assert.Equal(expectedZip, result.ZipCode);
//        Assert.Equal(expectedCity, result.City);
//        Assert.Equal(expectedCountry, result.Country);
//        Assert.Equal(expectedLon, result.Longitude, precision: 4);
//        Assert.Equal(expectedLat, result.Latitude, precision: 4);
//    }

//    [Fact]
//    public async Task FindByIdAsync_WithNullAdditionalInfo_ShouldReturnNullAdditionalInfo()
//    {
//        // Testdaten enthalten kein additional_info → Feld muss null sein
//        int addressId = GetAddressIdByRestaurant("Restaurant Nimmersatt");

//        var result = await _dao.FindByIdAsync(addressId);

//        Assert.NotNull(result);
//        Assert.Null(result.AdditionalInfo);
//    }

//    [Fact]
//    public async Task FindByIdAsync_WithAdditionalInfo_ShouldReturnAdditionalInfo()
//    {
//        var address = BuildTestAddress("Zusatzinfo Gasse", additionalInfo: "Top 5");
//        int newId = await _dao.InsertAsync(address);

//        var result = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(result);
//        Assert.Equal("Top 5", result.AdditionalInfo);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task FindByIdAsync_ShouldReturnAllMappedFields()
//    {
//        int addressId = GetAddressIdByRestaurant("Sakura Sushi");

//        var result = await _dao.FindByIdAsync(addressId);

//        Assert.NotNull(result);
//        Assert.True(result.Id > 0);
//        Assert.False(string.IsNullOrWhiteSpace(result.Street));
//        Assert.False(string.IsNullOrWhiteSpace(result.Number));
//        Assert.False(string.IsNullOrWhiteSpace(result.ZipCode));
//        Assert.False(string.IsNullOrWhiteSpace(result.City));
//        Assert.False(string.IsNullOrWhiteSpace(result.Country));
//    }

//    // -----------------------------------------------------------------------
//    // InsertAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task InsertAsync_ShouldReturnPositiveId()
//    {
//        var address = BuildTestAddress("Insert Straße");

//        int newId = await _dao.InsertAsync(address);

//        Assert.True(newId > 0);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_ShouldPersistAllFieldsInDatabase()
//    {
//        var address = BuildTestAddress("Persistenz Gasse", number: "42b",
//            zip: "9999", city: "Testburg", country: "Austria",
//            lon: 15.1234, lat: 47.5678, additionalInfo: "Stiege 3");

//        int newId = await _dao.InsertAsync(address);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Equal("Persistenz Gasse", fetched.Street);
//        Assert.Equal("42b", fetched.Number);
//        Assert.Equal("9999", fetched.ZipCode);
//        Assert.Equal("Testburg", fetched.City);
//        Assert.Equal("Austria", fetched.Country);
//        Assert.Equal(15.1234, fetched.Longitude, precision: 4);
//        Assert.Equal(47.5678, fetched.Latitude, precision: 4);
//        Assert.Equal("Stiege 3", fetched.AdditionalInfo);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_WithNullAdditionalInfo_ShouldPersistNull()
//    {
//        var address = BuildTestAddress("Null Info Weg", additionalInfo: null);

//        int newId = await _dao.InsertAsync(address);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Null(fetched.AdditionalInfo);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_TwoAddresses_ShouldReturnDistinctIds()
//    {
//        int id1 = await _dao.InsertAsync(BuildTestAddress("Distinct Straße 1"));
//        int id2 = await _dao.InsertAsync(BuildTestAddress("Distinct Straße 2"));

//        Assert.NotEqual(id1, id2);

//        CleanupAddress(id1);
//        CleanupAddress(id2);
//    }

//    [Fact]
//    public async Task InsertAsync_ShouldIncreaseTotalCountByOne()
//    {
//        int countBefore = GetCount("Address");

//        int newId = await _dao.InsertAsync(BuildTestAddress("Count Test Straße"));

//        Assert.Equal(countBefore + 1, GetCount("Address"));

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task InsertAsync_WithExtremeCoordinates_ShouldPersistCorrectly()
//    {
//        // Grenzwerte: Längengrad ±180, Breitengrad ±90
//        var address = BuildTestAddress("Extremkoord Allee", lon: 179.9999, lat: 89.9999);

//        int newId = await _dao.InsertAsync(address);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Equal(179.9999, fetched.Longitude, precision: 4);
//        Assert.Equal(89.9999, fetched.Latitude, precision: 4);

//        CleanupAddress(newId);
//    }

//    // -----------------------------------------------------------------------
//    // UpdateAsync
//    // -----------------------------------------------------------------------

//    [Fact]
//    public async Task UpdateAsync_WithValidAddress_ShouldReturnTrue()
//    {
//        int newId = await _dao.InsertAsync(BuildTestAddress("Update Return Test"));
//        var address = await _dao.FindByIdAsync(newId);
//        address!.Street = "Geänderte Straße";

//        bool result = await _dao.UpdateAsync(address);

//        Assert.True(result);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldPersistStreetChange()
//    {
//        int newId = await _dao.InsertAsync(BuildTestAddress("Original Straße"));
//        var address = await _dao.FindByIdAsync(newId);
//        address!.Street = "Neue Straße";

//        await _dao.UpdateAsync(address);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Equal("Neue Straße", fetched.Street);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldPersistAllFields()
//    {
//        int newId = await _dao.InsertAsync(BuildTestAddress("Vor Update Straße"));
//        var address = await _dao.FindByIdAsync(newId);

//        address!.Street = "Nach Update Straße";
//        address.Number = "99c";
//        address.ZipCode = "8888";
//        address.City = "Neustadt";
//        address.Country = "Germany";
//        address.Longitude = 13.4050;
//        address.Latitude = 52.5200;
//        address.AdditionalInfo = "EG links";

//        await _dao.UpdateAsync(address);
//        var fetched = await _dao.FindByIdAsync(newId);

//        Assert.NotNull(fetched);
//        Assert.Equal("Nach Update Straße", fetched.Street);
//        Assert.Equal("99c", fetched.Number);
//        Assert.Equal("8888", fetched.ZipCode);
//        Assert.Equal("Neustadt", fetched.City);
//        Assert.Equal("Germany", fetched.Country);
//        Assert.Equal(13.4050, fetched.Longitude, precision: 4);
//        Assert.Equal(52.5200, fetched.Latitude, precision: 4);
//        Assert.Equal("EG links", fetched.AdditionalInfo);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldAllowClearingAdditionalInfo()
//    {
//        var address = BuildTestAddress("Clear Info Straße", additionalInfo: "Vorher gesetzt");
//        int newId = await _dao.InsertAsync(address);

//        var fetched = await _dao.FindByIdAsync(newId);
//        fetched!.AdditionalInfo = null;
//        await _dao.UpdateAsync(fetched);

//        var afterUpdate = await _dao.FindByIdAsync(newId);
//        Assert.Null(afterUpdate!.AdditionalInfo);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldUpdateCoordinates()
//    {
//        int newId = await _dao.InsertAsync(BuildTestAddress("Koordinaten Straße", lon: 10.0, lat: 50.0));
//        var address = await _dao.FindByIdAsync(newId);

//        address!.Longitude = 16.3738;
//        address.Latitude = 48.2082;
//        await _dao.UpdateAsync(address);

//        var fetched = await _dao.FindByIdAsync(newId);
//        Assert.Equal(16.3738, fetched!.Longitude, precision: 4);
//        Assert.Equal(48.2082, fetched.Latitude, precision: 4);

//        CleanupAddress(newId);
//    }

//    [Fact]
//    public async Task UpdateAsync_WithNonExistentId_ShouldReturnFalse()
//    {
//        var ghost = new Address
//        {
//            Id = int.MaxValue,
//            Street = "Geisterstraße",
//            Number = "0",
//            ZipCode = "0000",
//            City = "Nirgendwo",
//            Country = "Nowhere",
//        };

//        bool result = await _dao.UpdateAsync(ghost);

//        Assert.False(result);
//    }

//    [Fact]
//    public async Task UpdateAsync_ShouldNotAffectOtherAddresses()
//    {
//        int id1 = await _dao.InsertAsync(BuildTestAddress("Unbeeinflusst Straße 1", city: "Stadt A"));
//        int id2 = await _dao.InsertAsync(BuildTestAddress("Unbeeinflusst Straße 2", city: "Stadt B"));

//        var address1 = await _dao.FindByIdAsync(id1);
//        address1!.City = "Stadt A – Geändert";
//        await _dao.UpdateAsync(address1);

//        var address2After = await _dao.FindByIdAsync(id2);
//        Assert.Equal("Stadt B", address2After!.City);

//        CleanupAddress(id1);
//        CleanupAddress(id2);
//    }

//    // -----------------------------------------------------------------------
//    // Hilfsmethoden
//    // -----------------------------------------------------------------------

//    private int GetFirstAddressId()
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand("SELECT TOP 1 id FROM Address ORDER BY id", conn);
//        return (int)cmd.ExecuteScalar();
//    }

//    private int GetAddressIdByRestaurant(string restaurantName)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand(@"
//            SELECT a.id FROM Address a
//            JOIN Restaurant r ON r.address_id = a.id
//            WHERE r.name = @name", conn);
//        cmd.Parameters.AddWithValue("@name", restaurantName);
//        var result = cmd.ExecuteScalar()
//            ?? throw new InvalidOperationException(
//                $"Kein Restaurant mit Name '{restaurantName}' gefunden.");
//        return (int)result;
//    }

//    private int GetCount(string tableName)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}]", conn);
//        return (int)cmd.ExecuteScalar();
//    }

//    private static Address BuildTestAddress(
//        string street,
//        string number = "1",
//        string zip = "0000",
//        string city = "Teststadt",
//        string country = "Austria",
//        double lon = 14.0,
//        double lat = 48.0,
//        string? additionalInfo = null) => new()
//        {
//            Street = street,
//            Number = number,
//            ZipCode = zip,
//            City = city,
//            Country = country,
//            Longitude = lon,
//            Latitude = lat,
//            AdditionalInfo = additionalInfo,
//        };

//    private void CleanupAddress(int id)
//    {
//        using var conn = new SqlConnection(_connectionString);
//        conn.Open();
//        using var cmd = new SqlCommand("DELETE FROM Address WHERE id = @id", conn);
//        cmd.Parameters.AddWithValue("@id", id);
//        cmd.ExecuteNonQuery();
//    }
//}