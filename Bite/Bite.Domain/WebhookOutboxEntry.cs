namespace Bite.Domain;

// A pending webhook waiting to be delivered. Lean read-model for the outbox worker.
public sealed record WebhookOutboxEntry(
    int Id,
    int OrderId,
    string Url,
    string Payload,
    int Attempts);
