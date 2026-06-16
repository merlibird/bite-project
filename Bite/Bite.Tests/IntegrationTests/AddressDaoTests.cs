using Bite.Dal.Ado;
using Bite.Dal.Common;
using Bite.Dal.Interface;
using Bite.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bite.Tests.IntegrationTests;

[Collection("Database")]
public class AddressDaoTests : IAsyncLifetime
{
    private readonly AddressDao dao;
    private readonly AdoTemplate template;

    public AddressDaoTests(DatabaseFixture fixture)
    {
        dao = new AddressDao(fixture.ConnectionFactory);
        template = new AdoTemplate(fixture.ConnectionFactory);
    }

    // beforeEach --> clear the Address table
    public async Task InitializeAsync()
    {
        await template.ExecuteAsync("delete from OrderStatusToken", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from OrderItem", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from CustomerOrder", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuItemMenuCategory", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuItem", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from MenuCategory", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from OpeningHourSlot", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from DeliveryFeeRule", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from DeliveryZone", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Restaurant", Array.Empty<QueryParameter>());
        await template.ExecuteAsync("delete from Address", Array.Empty<QueryParameter>());
    }

    // afterEach --> do nothing
    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await dao.FindByIdAsync(69420);
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_AllFieldsPersisted_CorrectlyMapped()
    {
        string street = "Hauptplatz";
        string number = "42";
        string zipCode = "1050";
        string city = "Wien";
        string country = "Österreich";
        double longitude = 47.06765146965344;
        double latitude = 13.862006980043356;
        string additionalInfo = "Top 2";

        var id = await dao.InsertAsync(MakeAddress(street, number, zipCode, city, country, longitude, latitude, additionalInfo));

        var result = await dao.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(street, result.Street);
        Assert.Equal(number, result.Number);
        Assert.Equal(zipCode, result.ZipCode);
        Assert.Equal(city, result.City);
        Assert.Equal(country, result.Country);
        Assert.Equal(longitude, result.Longitude);
        Assert.Equal(latitude, result.Latitude);
        Assert.Equal(additionalInfo, result.AdditionalInfo);
    }

    [Fact]
    public async Task FindByIdAsync_NullableFieldNotSet_ReturnsNull()
    {
        var id = await dao.InsertAsync(MakeAddress());

        var result = await dao.FindByIdAsync(id);

        Assert.NotNull(result);
        Assert.Null(result.AdditionalInfo);
    }

    // -------------------------------------------------------------------------
    // InsertAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InsertAsync_ValidAddress_ReturnsNewId()
    {
        var id = await dao.InsertAsync(MakeAddress());

        Assert.True(id > 0);
    }

    [Fact]
    public async Task InsertAsync_TwoAddresses_ReturnsDifferentIds()
    {
        var id1 = await dao.InsertAsync(MakeAddress("Hauptstraße", "1"));
        var id2 = await dao.InsertAsync(MakeAddress("Nebenstraße", "2"));

        Assert.NotEqual(id1, id2);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Address MakeAddress(
        string street = "Hauptstraße",
        string number = "1",
        string zipCode = "4040",
        string city = "Linz",
        string country = "Austria",
        double longitude = 48.06765146965344,
        double latitude = 12.862006980043356,
        string? additionalInfo = null) =>
            new Address(0, street, number, zipCode, city, country, longitude, latitude, additionalInfo);
    }
