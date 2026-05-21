using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Abstractions.Models;

namespace RabbitMQ.Abstractions.Implementation;

internal sealed class RabbitMqClient : IRabbitMqClient
{
    private readonly IChannelProvider _channelProvider;
    private readonly IMessagePublisher _publisher;
    private readonly IMessageConsumer _consumer;
    private readonly IQueueManager _queueManager;

    public bool IsConnected => _channelProvider.IsConnected;

    public RabbitMqClient(IChannelProvider channelProvider, IMessagePublisher publisher,
        IMessageConsumer consumer, IQueueManager queueManager)
    {
        _channelProvider = channelProvider;
        _publisher = publisher;
        _consumer = consumer;
        _queueManager = queueManager;
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
        => _channelProvider.ConnectAsync(cancellationToken);

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
        => _channelProvider.DisconnectAsync(cancellationToken);

    // IMessagePublisher delegation
    public Task PublishAsync<T>(string exchange, string routingKey, T message,
        CancellationToken cancellationToken = default) where T : class
        => _publisher.PublishAsync(exchange, routingKey, message, cancellationToken);

    public Task PublishAsync<T>(string exchange, string routingKey, T message,
        MessageProperties properties, CancellationToken cancellationToken = default) where T : class
        => _publisher.PublishAsync(exchange, routingKey, message, properties, cancellationToken);

    public Task PublishBatchAsync<T>(string exchange, string routingKey, IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : class
        => _publisher.PublishBatchAsync(exchange, routingKey, messages, cancellationToken);

    // IMessageConsumer delegation
    public Task SubscribeAsync<T>(string queue, Func<MessageEnvelope<T>, Task> handler,
        ConsumerOptions? options = null, CancellationToken cancellationToken = default) where T : class
        => _consumer.SubscribeAsync(queue, handler, options, cancellationToken);

    public Task UnsubscribeAsync(string queue, CancellationToken cancellationToken = default)
        => _consumer.UnsubscribeAsync(queue, cancellationToken);

    public Task<MessageEnvelope<T>?> GetMessageAsync<T>(string queue, bool autoAck = false,
        CancellationToken cancellationToken = default) where T : class
        => _consumer.GetMessageAsync<T>(queue, autoAck, cancellationToken);

    public Task AckAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
        => _consumer.AckAsync(deliveryTag, cancellationToken);

    public Task NackAsync(ulong deliveryTag, bool requeue = true, CancellationToken cancellationToken = default)
        => _consumer.NackAsync(deliveryTag, requeue, cancellationToken);

    // IQueueManager delegation
    public Task DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
        => _queueManager.DeclareQueueAsync(queue, durable, exclusive, autoDelete, arguments, cancellationToken);

    public Task DeclareExchangeAsync(string exchange, string type, bool durable = true,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
        => _queueManager.DeclareExchangeAsync(exchange, type, durable, autoDelete, arguments, cancellationToken);

    public Task BindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
        => _queueManager.BindQueueAsync(queue, exchange, routingKey, arguments, cancellationToken);

    public Task UnbindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
        => _queueManager.UnbindQueueAsync(queue, exchange, routingKey, arguments, cancellationToken);

    public Task DeleteQueueAsync(string queue, bool ifUnused = false, bool ifEmpty = false,
        CancellationToken cancellationToken = default)
        => _queueManager.DeleteQueueAsync(queue, ifUnused, ifEmpty, cancellationToken);

    public Task DeleteExchangeAsync(string exchange, bool ifUnused = false,
        CancellationToken cancellationToken = default)
        => _queueManager.DeleteExchangeAsync(exchange, ifUnused, cancellationToken);

    public Task PurgeQueueAsync(string queue, CancellationToken cancellationToken = default)
        => _queueManager.PurgeQueueAsync(queue, cancellationToken);

    public Task<uint> GetMessageCountAsync(string queue, CancellationToken cancellationToken = default)
        => _queueManager.GetMessageCountAsync(queue, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
