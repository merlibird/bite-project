using Bite.Services.Interface;
using System.Text;

namespace Bite.Services.Implementation;

public sealed class WebhookSender : IWebhookSender, IDisposable
{
    private readonly HttpClient httpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2) // makes sure DNS changes are respected eventually
    })
    { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<bool> SendAsync(string webhookUrl, string jsonPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(webhookUrl, content, cancellationToken);
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
