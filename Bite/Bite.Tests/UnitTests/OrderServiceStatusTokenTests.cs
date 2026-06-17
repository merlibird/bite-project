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

    public OrderServiceStatusTokenTests()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>()).Returns(Order());
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>()).Returns(StatusToken());
        orderStatusTokenDao.MarkUsedAsync(TokenId, Arg.Any<CancellationToken>()).Returns(true);
    }

    private static CustomerOrder Order(int restaurantId = RestaurantId, OrderStatus status = OrderStatus.Received)
        => new(OrderId, restaurantId, addressId: 1, OrderCode, status, deliveryFee: 0, total: 0);

    private static OrderStatusToken StatusToken(bool used = false, int orderId = OrderId, DateTime? expiresAt = null, OrderStatus targetStatus = TargetStatus)
        => new(TokenId, orderId, Token, targetStatus, used, expiresAt ?? DateTime.UtcNow.AddMinutes(10));

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
    public async Task ApplyStatusTokenAsync_OrderBelongsToOtherRestaurant_ReturnsForbidden()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>())
            .Returns(Order(restaurantId: 999));

        var result = await Apply();

        Assert.Equal(ServiceResultType.Forbidden, result.ResultType);
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
    public async Task ApplyStatusTokenAsync_InvalidTransition_ReturnsConflict()
    {
        // Try to go from Delivered to InPreparation
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>())
            .Returns(Order(status: OrderStatus.Delivered));
        
        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns(StatusToken(targetStatus: OrderStatus.InPreparation));

        var result = await Apply();

        Assert.Equal(ServiceResultType.Conflict, result.ResultType);
    }

    [Fact]
    public async Task ApplyStatusTokenAsync_ValidToken_UpdatesOrderToTargetStatus()
    {
        customerOrderDao.FindByOrderCodeAsync(OrderCode, Arg.Any<CancellationToken>())
            .Returns(Order(status: OrderStatus.SentToRestaurant));

        orderStatusTokenDao.FindByTokenAsync(Token, Arg.Any<CancellationToken>())
            .Returns(StatusToken(targetStatus: OrderStatus.InPreparation));

        await Apply();

        await customerOrderDao.Received(1)
            .UpdateStatusAsync(OrderId, OrderStatus.InPreparation, Arg.Any<CancellationToken>());
    }
}
