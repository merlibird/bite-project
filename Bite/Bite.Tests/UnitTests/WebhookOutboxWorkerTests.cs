using Bite.Api.Webhooks;
using Bite.Dal.Interface;
using Bite.Domain;
using Bite.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

// This test class was created with the help and assistance of AI 
namespace Bite.Tests.UnitTests;

public class WebhookOutboxWorkerTests
{
    // Mirrors WebhookOutboxWorker.RetryDelaysSeconds = [0, 2, 5, 15, 30, 60] (6 attempts).
    private const int MaxAttempts = 6;

    private readonly IWebhookOutboxDao outboxDao = Substitute.For<IWebhookOutboxDao>();
    private readonly ICustomerOrderDao orderDao = Substitute.For<ICustomerOrderDao>();
    private readonly IWebhookSender sender = Substitute.For<IWebhookSender>();

    private WebhookOutboxWorker CreateWorker()
    {
        var services = new ServiceCollection();
        services.AddSingleton(outboxDao);
        services.AddSingleton(orderDao);
        var provider = services.BuildServiceProvider();
        return new WebhookOutboxWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            sender,
            NullLogger<WebhookOutboxWorker>.Instance);
    }

    private void SetDueEntries(params WebhookOutboxEntry[] entries)
        => outboxDao.FindDueAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(entries);

    private void SenderReturns(bool delivered)
        => sender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(delivered);

    private static WebhookOutboxEntry Entry(int id = 1, int orderId = 100, int attempts = 0)
        => new(id, orderId, "https://hook", "{}", attempts);

    private Task Process() => CreateWorker().ProcessDueAsync(CancellationToken.None);

    // =====================================================================
    // Successful delivery
    // =====================================================================

    [Fact]
    public async Task ProcessDueAsync_SuccessfulDelivery_MarksSentAndAdvancesOrderStatus()
    {
        SetDueEntries(Entry(id: 1, orderId: 100));
        SenderReturns(true);

        await Process();

        await outboxDao.Received(1).MarkSentAsync(1, Arg.Any<CancellationToken>());
        await orderDao.Received(1).UpdateStatusIfAsync(
            100, OrderStatus.Received, OrderStatus.SentToRestaurant, Arg.Any<CancellationToken>());
        await outboxDao.DidNotReceive().RescheduleAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessDueAsync_MultipleEntries_ProcessesEach()
    {
        SetDueEntries(Entry(id: 1, orderId: 100), Entry(id: 2, orderId: 200));
        SenderReturns(true);

        await Process();

        await outboxDao.Received(1).MarkSentAsync(1, Arg.Any<CancellationToken>());
        await outboxDao.Received(1).MarkSentAsync(2, Arg.Any<CancellationToken>());
    }

    // =====================================================================
    // Failed delivery: retry vs. permanent failure
    // =====================================================================

    [Fact]
    public async Task ProcessDueAsync_FailedDeliveryBelowMaxAttempts_ReschedulesWithBackoff()
    {
        SetDueEntries(Entry(id: 1, attempts: 0));
        SenderReturns(false);

        await Process();

        // attempts -> 1, RetryDelaysSeconds[1] == 2
        await outboxDao.Received(1).RescheduleAsync(1, 1, 2, Arg.Any<CancellationToken>());
        await outboxDao.DidNotReceive().MarkFailedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await outboxDao.DidNotReceive().MarkSentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessDueAsync_FailedDeliveryAtMaxAttempts_MarksPermanentlyFailed()
    {
        SetDueEntries(Entry(id: 1, attempts: MaxAttempts - 1)); // next attempt == MaxAttempts
        SenderReturns(false);

        await Process();

        await outboxDao.Received(1).MarkFailedAsync(1, MaxAttempts, Arg.Any<CancellationToken>());
        await outboxDao.DidNotReceive().RescheduleAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
