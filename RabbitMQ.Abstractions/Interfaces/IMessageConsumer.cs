using RabbitMQ.Abstractions.Models;

namespace RabbitMQ.Abstractions.Interfaces;

public interface IMessageConsumer
{
    Task SubscribeAsync<T>(string queue, Func<MessageEnvelope<T>, Task> handler,
        ConsumerOptions? options = null, CancellationToken cancellationToken = default) where T : class;

    Task UnsubscribeAsync(string queue, CancellationToken cancellationToken = default);

    Task<MessageEnvelope<T>?> GetMessageAsync<T>(string queue, bool autoAck = false,
        CancellationToken cancellationToken = default) where T : class;

    Task AckAsync(ulong deliveryTag, CancellationToken cancellationToken = default);

    Task NackAsync(ulong deliveryTag, bool requeue = true, CancellationToken cancellationToken = default);
}
