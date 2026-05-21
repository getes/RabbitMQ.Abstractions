using RabbitMQ.Abstractions.Models;

namespace RabbitMQ.Abstractions.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message,
        CancellationToken cancellationToken = default) where T : class;

    Task PublishAsync<T>(string exchange, string routingKey, T message,
        MessageProperties properties, CancellationToken cancellationToken = default) where T : class;

    Task PublishBatchAsync<T>(string exchange, string routingKey, IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : class;
}
