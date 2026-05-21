namespace RabbitMQ.Abstractions.Interfaces;

public interface IQueueManager
{
    Task DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default);

    Task DeclareExchangeAsync(string exchange, string type, bool durable = true,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default);

    Task BindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default);

    Task UnbindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default);

    Task DeleteQueueAsync(string queue, bool ifUnused = false, bool ifEmpty = false,
        CancellationToken cancellationToken = default);

    Task DeleteExchangeAsync(string exchange, bool ifUnused = false,
        CancellationToken cancellationToken = default);

    Task PurgeQueueAsync(string queue, CancellationToken cancellationToken = default);

    Task<uint> GetMessageCountAsync(string queue, CancellationToken cancellationToken = default);
}
