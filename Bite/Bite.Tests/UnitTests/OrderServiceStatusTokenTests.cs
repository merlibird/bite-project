using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Common;
using Bite.Services.Implementation;
using Bite.Services.Interface;
using NSubstitute;

namespace Bite.Tests.UnitTests;

public class OrderServiceStatusTokenTests
{
    private const string OrderCode = "ABCD2345";
    private const int OrderId = 42;
    private const int RestaurantId = 1;
    private const int TokenId = 7;
    private const string Token = "tok";
    private const OrderStatus TargetStatus = OrderStatus.InPreparation;

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
        orderWebhookService);

    public OrderServiceStatusTokenTests()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>()).Returns(Order());
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>()).Returns(StatusToken());
        orderStatusTokenDao.MarkUsedAsync(TokenId, Arg.Any<CancellationToken>()).Returns(true);
    }

    private static CustomerOrder Order(int restaurantId = RestaurantId)
        => new(OrderId, restaurantId, addressId: 1, OrderCode, OrderStatus.Received, deliveryFee: 0, total: 0);

    private static OrderStatusToken StatusToken(bool used = false, int orderId = OrderId, DateTime? expiresAt = null)
        => new(TokenId, orderId, Token, TargetStatus, used, expiresAt ?? DateTime.UtcNow.AddMinutes(10));

    private Task<ServiceResult<OrderStatus>> Apply()
        => CreateService().ApplyStatusTokenAsync(OrderCode, Token, RestaurantId);

    [Fact]
    public async Task ApplyStatusTokenAsync_OrderNotFound_ReturnsNotFound()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>())
            .Returns((CustomerOrder?)null);

        var result = await Apply();

        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_OrderBelongsToOtherRestaurant_ReturnsNotFound()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>())
            .Returns(Order(restaurantId: 999));

        var result = await Apply();

        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_TokenNotFound_ReturnsNotFound()
    {
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns((OrderStatusToken?)null);

        var result = await Apply();

        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_TokenBelongsToOtherOrder_ReturnsNotFound()
    {
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns(StatusToken(orderId: 9999));

        var result = await Apply();

        Assert.Equal(ServiceResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_AlreadyUsedToken_ReturnsConflict()
    {
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns(StatusToken(used: true));

        var result = await Apply();

        Assert.Equal(ServiceResultType.Conflict, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_ExpiredToken_ReturnsConflict()
    {
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns(StatusToken(expiresAt: DateTime.UtcNow.AddMinutes(-1)));

        var result = await Apply();

        Assert.Equal(ServiceResultType.Conflict, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_ValidToken_ReturnsSuccessWithTargetStatus()
    {
        var result = await Apply();

        Assert.Equal(ServiceResultType.Success, result.ResultType);
        Assert.Equal(TargetStatus, result.Data);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_ValidToken_UpdatesOrderToTargetStatus()
    {
        await Apply();

        await customerOrderDao.Received(1)
            .UpdateStatusAsync(OrderId, TargetStatus, Arg.Any<CancellationToken>());
    }
}
