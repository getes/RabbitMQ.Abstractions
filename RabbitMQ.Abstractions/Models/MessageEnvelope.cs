namespace RabbitMQ.Abstractions.Models;

public class MessageEnvelope<T> where T : class
{
    public required T Body { get; init; }
    public string? MessageId { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public IDictionary<string, object?> Headers { get; init; } = new Dictionary<string, object?>();
    public string? Exchange { get; init; }
    public string? RoutingKey { get; init; }
    public ulong DeliveryTag { get; init; }
    public bool Redelivered { get; init; }
}
