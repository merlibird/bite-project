using Bite.Services.Interface;
using System.Net.Http.Json;

namespace Bite.Services.Implementation;

public sealed class WebhookSender : IWebhookSender, IDisposable
{
    private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<bool> SendAsync(string webhookUrl, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    public void Dispose() => httpClient.Dispose();
}
