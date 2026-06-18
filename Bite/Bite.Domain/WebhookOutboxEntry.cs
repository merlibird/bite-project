namespace Bite.Domain;

// webhook logic was created with the help of AI
public sealed record WebhookOutboxEntry(
    int Id,
    int OrderId,
    string Url,
    string Payload,
    int Attempts);
