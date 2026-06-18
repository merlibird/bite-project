using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Implementation;
using NSubstitute;
using System.Security.Cryptography;

namespace Bite.Tests.UnitTests;

public class OrderStatusTokenServiceTests
{
    private readonly IOrderStatusTokenDao orderStatusTokenDao = Substitute.For<IOrderStatusTokenDao>();

    private OrderStatusTokenService CreateService() => new(orderStatusTokenDao);

    [Fact]
    public async Task CreateTokensForOrderAsync_InsertsTokensForAllRelevantStatuses()
    {
        const int orderId = 123;
        var service = CreateService();

        var tokens = await service.CreateTokensForOrderAsync(orderId);

        var expectedStatuses = new[]
        {
            OrderStatus.SentToRestaurant,
            OrderStatus.InPreparation,
            OrderStatus.OutForDelivery,
            OrderStatus.Delivered,
            OrderStatus.Cancelled
        };

        Assert.Equal(expectedStatuses.Length, tokens.Count);
        foreach (var status in expectedStatuses)
        {
            Assert.True(tokens.ContainsKey(status));
            Assert.False(string.IsNullOrWhiteSpace(tokens[status]));
            
            await orderStatusTokenDao.Received(1).InsertAsync(
                Arg.Is<OrderStatusToken>(t => t.OrderId == orderId && t.TargetStatus == status && !t.Used),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task CreateTokensForOrderAsync_GeneratesUniqueTokens()
    {
        const int orderId = 123;
        var service = CreateService();

        var tokens = await service.CreateTokensForOrderAsync(orderId);

        var distinctTokenValues = tokens.Values.Distinct().Count();
        Assert.Equal(tokens.Count, distinctTokenValues);
    }
}
