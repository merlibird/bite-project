using Bite.Services.Interface;
using System.Collections.Concurrent;

namespace Bite.Services.Implementation;

public sealed class WebhookQueue : IWebhookQueue
{
    private readonly ConcurrentQueue<WebhookJob> jobs = new();

    public void Enqueue(WebhookJob job) => jobs.Enqueue(job);

    public WebhookJob? Dequeue() => jobs.TryDequeue(out var job) ? job : null;
}
