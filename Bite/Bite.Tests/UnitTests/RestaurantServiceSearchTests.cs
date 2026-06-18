using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Implementation;
using Bite.Services.Interface;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.UnitTests;

public class RestaurantServiceSearchTests
{
    private readonly IRestaurantDao restaurantDao = Substitute.For<IRestaurantDao>();
    private readonly IAddressDao addressDao = Substitute.For<IAddressDao>();
    private readonly IOpeningHourSlotDao openingHourSlotDao = Substitute.For<IOpeningHourSlotDao>();
    private readonly IApiKeyService apiKeyService = Substitute.For<IApiKeyService>();
    private readonly IDeliveryZoneDao deliveryZoneDao = Substitute.For<IDeliveryZoneDao>();
    private readonly IDeliveryFeeRuleDao deliveryFeeRuleDao = Substitute.For<IDeliveryFeeRuleDao>();
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 6, 17, 12, 0, 0, TimeSpan.Zero)); // Wednesday 12:00

    private readonly List<Restaurant> restaurants = [];
    private readonly List<Address> addresses = [];
    private readonly List<OpeningHourSlot> openingHours = [];
    private readonly List<DeliveryZone> deliveryZones = [];

    public RestaurantServiceSearchTests()
    {
        timeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
        restaurantDao.FindAllAsync(Arg.Any<CancellationToken>()).Returns(_ => restaurants);
        addressDao.FindAllAsync(Arg.Any<CancellationToken>()).Returns(_ => addresses);
        openingHourSlotDao.FindAllAsync(Arg.Any<CancellationToken>()).Returns(_ => openingHours);
        deliveryZoneDao.FindAllAsync(Arg.Any<CancellationToken>()).Returns(_ => deliveryZones);
    }

    private RestaurantService CreateService() => new(
        restaurantDao, addressDao, openingHourSlotDao, apiKeyService,
        timeProvider, deliveryZoneDao, deliveryFeeRuleDao);

    // Adds a restaurant at the given latitude (longitude 0). Open all Wednesday by default,
    // reachable within a 5 km delivery zone unless overridden.
    private void AddRestaurant(int id, double latitude, bool open = true, bool reachable = true, bool hasAddress = true)
    {
        int addressId = id * 10;
        restaurants.Add(new Restaurant(id, $"R{id}", addressId, "https://hook", "key"));

        if (hasAddress)
        {
            addresses.Add(new Address(addressId, "S", "1", "1010", "Vienna", "AT", longitude: 0, latitude: latitude));
        }

        openingHours.Add(open
            ? new OpeningHourSlot(0, id, 3, TimeSpan.Zero, new TimeSpan(23, 0, 0))  // Wed 00:00–23:00
            : new OpeningHourSlot(0, id, 1, TimeSpan.Zero, new TimeSpan(23, 0, 0))); // Mon only -> closed

        deliveryZones.Add(new DeliveryZone(id, id, minOrderValue: 0, maxDistance: reachable ? 5 : 0.001));
    }

    // Searches from the origin (0,0), where the configured restaurants sit a few km north.
    private Task<IReadOnlyCollection<(
        Restaurant Restaurant,
        Address Address,
        double DistanceInKm,
        bool IsOpenNow,
        IReadOnlyCollection<OpeningHourSlot> OpeningHours)>> Search(bool openNowOnly = false, int count = 10)
        => CreateService().SearchRestaurantsAsync(0, 0, openNowOnly, count);

    // =====================================================================
    // Delivery-zone filter
    // =====================================================================

    [Fact]
    public async Task SearchRestaurantsAsync_RestaurantOutsideDeliveryZone_IsExcluded()
    {
        AddRestaurant(1, latitude: 0.01);                   // ~1.1 km, reachable
        AddRestaurant(2, latitude: 1.0, reachable: false);  // ~111 km, zone only 0.001 km

        var result = await Search();

        Assert.Single(result);
        Assert.Equal(1, result.Single().Restaurant.Id);
    }

    // =====================================================================
    // Sorting & result limit
    // =====================================================================

    [Fact]
    public async Task SearchRestaurantsAsync_OrdersResultsByDistanceAscending()
    {
        AddRestaurant(1, latitude: 0.02); // ~2.2 km
        AddRestaurant(2, latitude: 0.01); // ~1.1 km

        var result = await Search();

        Assert.Equal([2, 1], result.Select(r => r.Restaurant.Id).ToArray());
    }

    [Fact]
    public async Task SearchRestaurantsAsync_Count_LimitsNumberOfResults()
    {
        AddRestaurant(1, latitude: 0.01);
        AddRestaurant(2, latitude: 0.02);
        AddRestaurant(3, latitude: 0.03);

        var result = await Search(count: 2);

        Assert.Equal(2, result.Count);
    }

    // =====================================================================
    // Open-now filter
    // =====================================================================

    [Fact]
    public async Task SearchRestaurantsAsync_OpenNowOnly_ExcludesClosedRestaurants()
    {
        AddRestaurant(1, latitude: 0.01, open: false);
        AddRestaurant(2, latitude: 0.02, open: true);

        var result = await Search(openNowOnly: true);

        Assert.Single(result);
        Assert.Equal(2, result.Single().Restaurant.Id);
        Assert.True(result.Single().IsOpenNow);
    }

    [Fact]
    public async Task SearchRestaurantsAsync_NotOpenNowOnly_IncludesClosedRestaurantsWithFlag()
    {
        AddRestaurant(1, latitude: 0.01, open: false);

        var result = await Search(openNowOnly: false);

        Assert.Single(result);
        Assert.False(result.Single().IsOpenNow);
    }
}
