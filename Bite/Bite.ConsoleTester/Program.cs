using Bite.Services.Implementation;

const string webhookUrl = "https://webhook.site/075579c0-49f3-4b69-89df-2efd24fe250a";

Console.WriteLine("Testing WebhookSender...");

// Sample payload, roughly shaped like a future order webhook (#9).
var payload = new
{
    orderCode = "7F3K9A2X",
    status = "RECEIVED",
    deliveryAddress = new
    {
        street = "Softwarepark",
        number = "11",
        zipCode = "4232",
        city = "Hagenberg",
    },
    items = new[]
    {
        new { name = "Margherita", quantity = 2, unitPrice = 9.50m },
        new { name = "Cola",       quantity = 1, unitPrice = 2.50m },
    },
    deliveryFee = 0.00m,
    total = 21.50m,
};

var sender = new WebhookSender();

bool delivered = await sender.SendAsync(webhookUrl, payload);

Console.WriteLine(delivered
    ? "Webhook delivered (2xx). Check webhook.site for the incoming POST."
    : "Webhook NOT delivered (non-2xx / unreachable / timeout).");
