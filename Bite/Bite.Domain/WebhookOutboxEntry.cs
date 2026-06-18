namespace Bite.Domain;

// webhook logic was created with the help of AI
// A pending webhook waiting to be delivered. Lean read-model for the outbox worker.
public sealed record WebhookOutboxEntry(
    int Id,
    int OrderId,
    string Url,
    string Payload,
    int Attempts);
