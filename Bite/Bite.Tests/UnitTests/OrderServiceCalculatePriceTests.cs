using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Implementation;
using Bite.Services.Interface;
using NSubstitute;

namespace Bite.Tests.UnitTests;

public class OrderServiceCalculatePriceTests
{
    private const int RestaurantId = 1;
    private const int RestaurantAddressId = 10;
    private const int MenuItemId = 100;
    private const decimal ItemPrice = 10m;

    private const int NearZoneId = 5;   // "unter 10 km"
    private const int FarZoneId = 6;    // "ab 10 km"
    private const double FarZoneMaxKm = 50; // test-only finite cap (spec says "ab 10 km" is open-ended)

    // Restaurant sits at (0,0). Delivery latitude offsets at longitude 0: 1° ≈ 111.19 km.
    private const double NearLat = 0.045;    // ~5 km  -> under 10 km zone
    private const double FarLat = 0.135;     // ~15 km -> from 10 km zone
    private const double OutsideLat = 0.5;   // ~55 km -> beyond all zones

    private readonly ICustomerOrderDao customerOrderDao = Substitute.For<ICustomerOrderDao>();
    private readonly IOrderStatusTokenDao orderStatusTokenDao = Substitute.For<IOrderStatusTokenDao>();
    private readonly IRestaurantDao restaurantDao = Substitute.For<IRestaurantDao>();
    private readonly IAddressDao addressDao = Substitute.For<IAddressDao>();
    private readonly IMenuItemDao menuItemDao = Substitute.For<IMenuItemDao>();
    private readonly IDeliveryZoneDao deliveryZoneDao = Substitute.For<IDeliveryZoneDao>();
    private readonly IDeliveryFeeRuleDao deliveryFeeRuleDao = Substitute.For<IDeliveryFeeRuleDao>();
    private readonly IOrderCodeService orderCodeService = Substitute.For<IOrderCodeService>();
    private readonly IOrderItemDao orderItemDao = Substitute.For<IOrderItemDao>();
    private readonly IOrderWebhookService orderWebhookService = Substitute.For<IOrderWebhookService>();
    private readonly IOrderStatusTokenService orderStatusTokenService = Substitute.For<IOrderStatusTokenService>();

    private OrderService CreateService() => new(
        customerOrderDao,
        orderStatusTokenDao,
        restaurantDao,
        addressDao,
        menuItemDao,
        deliveryZoneDao,
        deliveryFeeRuleDao,
        orderCodeService,
        orderItemDao,
        orderWebhookService,
        orderStatusTokenService);

    public OrderServiceCalculatePriceTests()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new Restaurant(RestaurantId, "Test", RestaurantAddressId, "https://hook", "key"));
        addressDao.FindByIdAsync(RestaurantAddressId, Arg.Any<CancellationToken>())
            .Returns(Address(0, 0));

        menuItemDao.FindByIdAsync(MenuItemId, Arg.Any<CancellationToken>())
            .Returns(new MenuItem(MenuItemId, RestaurantId, "Pizza", null, ItemPrice, isActive: true));

        deliveryZoneDao.FindByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new DeliveryZone(NearZoneId, RestaurantId, minOrderValue: 20m, maxDistance: 10),
                new DeliveryZone(FarZoneId, RestaurantId, minOrderValue: 20m, maxDistance: FarZoneMaxKm),
            });
        // Under 10 km: no delivery fee.
        deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(RestaurantId, NearZoneId, Arg.Any<CancellationToken>())
            .Returns(new[] { new DeliveryFeeRule(1, NearZoneId, maxOrderValue: 1_000_000m, deliveryFee: 0m) });
        // From 10 km: up to 30 € => 5 €, above 30 € => free
        deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(RestaurantId, FarZoneId, Arg.Any<CancellationToken>())
            .Returns(new[] { new DeliveryFeeRule(2, FarZoneId, maxOrderValue: 30m, deliveryFee: 5m) });
    }

    private static Address Address(double lat, double lon)
        => new(RestaurantAddressId, "Street", "1", "1010", "Vienna", "AT", longitude: lon, latitude: lat);

    private Task<ServiceResult<OrderPriceResponse>> CalculateAt(double latitude, params (int, int)[] items)
        => CreateService().CalculatePriceAsync(RestaurantId, items, (latitude, 0));

    // =====================================================================
    // Subtotal & total
    // =====================================================================

    [Fact]
    public async Task CalculatePriceAsync_ValidOrder_ReturnsSubtotalFeeAndTotal()
    {
        var result = await CalculateAt(FarLat, (MenuItemId, 3)); // ~15 km, subtotal 30 -> fee 5

        Assert.Equal(ServiceResultType.Success, result.ResultType);
        Assert.Equal(30m, result.Data!.Subtotal);
        Assert.Equal(5m, result.Data.DeliveryFee);
        Assert.Equal(35m, result.Data.TotalPrice);
    }

    [Fact]
    public async Task CalculatePriceAsync_MultipleItems_SumsSubtotal()
    {
        const int secondItemId = 101;
        menuItemDao.FindByIdAsync(secondItemId, Arg.Any<CancellationToken>())
            .Returns(new MenuItem(secondItemId, RestaurantId, "Cola", null, price: 5m, isActive: true));

        var result = await CalculateAt(NearLat, (MenuItemId, 2), (secondItemId, 4)); // 20 + 20 = 40

        Assert.True(result.IsSuccess);
        Assert.Equal(40m, result.Data!.Subtotal);
    }

    // =====================================================================
    // Restaurant / address lookups
    // =====================================================================

    [Fact]
    public async Task CalculatePriceAsync_RestaurantNotFound_ReturnsNotFound()
    {
        restaurantDao.FindByIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns((Restaurant?)null);

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_RestaurantAddressMissing_ReturnsError()
    {
        addressDao.FindByIdAsync(RestaurantAddressId, Arg.Any<CancellationToken>())
            .Returns((Address?)null);

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    // =====================================================================
    // Item validation
    // =====================================================================

    [Fact]
    public async Task CalculatePriceAsync_EmptyOrder_ReturnsError()
    {
        var result = await CalculateAt(NearLat); // no items

        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_UnknownMenuItem_ReturnsError()
    {
        menuItemDao.FindByIdAsync(MenuItemId, Arg.Any<CancellationToken>())
            .Returns((MenuItem?)null);

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_InactiveMenuItem_ReturnsError()
    {
        menuItemDao.FindByIdAsync(MenuItemId, Arg.Any<CancellationToken>())
            .Returns(new MenuItem(MenuItemId, RestaurantId, "Pizza", null, ItemPrice, isActive: false));

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_MenuItemFromOtherRestaurant_ReturnsError()
    {
        menuItemDao.FindByIdAsync(MenuItemId, Arg.Any<CancellationToken>())
            .Returns(new MenuItem(MenuItemId, restaurantId: 999, "Pizza", null, ItemPrice, isActive: true));

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.Error, result.ResultType);
    }

    // =====================================================================
    // Delivery conditions driven by distance & order value
    // =====================================================================

    [Fact]
    public async Task CalculatePriceAsync_Under10Km_HasNoDeliveryFee()
    {
        var result = await CalculateAt(NearLat, (MenuItemId, 3)); // ~5 km, subtotal 30

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Data!.DeliveryFee);
    }

    [Fact]
    public async Task CalculatePriceAsync_From10Km_OrderUpTo30_Costs5()
    {
        var result = await CalculateAt(FarLat, (MenuItemId, 3)); // ~15 km, subtotal exactly 30

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, result.Data!.DeliveryFee);
    }

    [Fact]
    public async Task CalculatePriceAsync_From10Km_OrderAbove30_IsFree()
    {
        var result = await CalculateAt(FarLat, (MenuItemId, 4)); // ~15 km, subtotal 40 > 30

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Data!.DeliveryFee);
    }

    [Fact]
    public async Task CalculatePriceAsync_BelowMinimumOrderValue_ReturnsValidationError()
    {
        var result = await CalculateAt(NearLat, (MenuItemId, 1)); // subtotal 10 < 20 €

        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_OutsideAllZones_ReturnsValidationError()
    {
        var result = await CalculateAt(OutsideLat, (MenuItemId, 3)); // ~55 km, beyond 50 km cap

        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_NearestZoneBelowMinOrderValue_SelectsLargerZone()
    {
        deliveryZoneDao.FindByRestaurantIdAsync(RestaurantId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new DeliveryZone(NearZoneId, RestaurantId, minOrderValue: 1000m, maxDistance: 2),
                new DeliveryZone(FarZoneId, RestaurantId, minOrderValue: 20m, maxDistance: 10),
            });
        deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(RestaurantId, FarZoneId, Arg.Any<CancellationToken>())
            .Returns(new[] { new DeliveryFeeRule(2, FarZoneId, maxOrderValue: 100m, deliveryFee: 7m) });

        var result = await CalculateAt(0, (MenuItemId, 3)); // 0 km, subtotal 30

        Assert.True(result.IsSuccess);
        Assert.Equal(7m, result.Data!.DeliveryFee);
    }

    [Fact]
    public async Task CalculatePriceAsync_NoFeeRulesForZone_ReturnsValidationError()
    {
        deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(RestaurantId, NearZoneId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DeliveryFeeRule>());

        var result = await CalculateAt(NearLat, (MenuItemId, 3));

        Assert.Equal(ServiceResultType.ValidationError, result.ResultType);
    }

    [Fact]
    public async Task CalculatePriceAsync_MultipleFeeTiers_AppliesCheapestCoveringTier()
    {
        deliveryFeeRuleDao.FindByRestaurantIdAndZoneIdAsync(RestaurantId, NearZoneId, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new DeliveryFeeRule(1, NearZoneId, maxOrderValue: 25m, deliveryFee: 5m),
                new DeliveryFeeRule(2, NearZoneId, maxOrderValue: 50m, deliveryFee: 2m),
            });

        var result = await CalculateAt(NearLat, (MenuItemId, 3)); 

        Assert.True(result.IsSuccess);
        Assert.Equal(2m, result.Data!.DeliveryFee);
    }
}
