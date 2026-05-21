using Microsoft.Extensions.Logging;
using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Client;

namespace RabbitMQ.Abstractions.Implementation;

internal sealed class QueueManager : IQueueManager
{
    private readonly IChannelProvider _channelProvider;
    private readonly ILogger<QueueManager> _logger;

    public QueueManager(IChannelProvider channelProvider, ILogger<QueueManager> logger)
    {
        _channelProvider = channelProvider;
        _logger = logger;
    }

    public async Task DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments ?? new Dictionary<string, object?>(), false, false, cancellationToken);
        _logger.LogDebug("Declared queue '{Queue}' (durable={Durable})", queue, durable);
    }

    public async Task DeclareExchangeAsync(string exchange, string type, bool durable = true,
        bool autoDelete = false, IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete, arguments ?? new Dictionary<string, object?>(), false, false, cancellationToken);
        _logger.LogDebug("Declared exchange '{Exchange}' (type={Type}, durable={Durable})", exchange, type, durable);
    }

    public async Task BindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.QueueBindAsync(queue, exchange, routingKey, arguments ?? new Dictionary<string, object?>(), false, cancellationToken);
        _logger.LogDebug("Bound queue '{Queue}' to exchange '{Exchange}' with key '{RoutingKey}'", queue, exchange, routingKey);
    }

    public async Task UnbindQueueAsync(string queue, string exchange, string routingKey,
        IDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.QueueUnbindAsync(queue, exchange, routingKey, arguments ?? new Dictionary<string, object?>(), cancellationToken);
        _logger.LogDebug("Unbound queue '{Queue}' from exchange '{Exchange}' with key '{RoutingKey}'", queue, exchange, routingKey);
    }

    public async Task DeleteQueueAsync(string queue, bool ifUnused = false, bool ifEmpty = false,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.QueueDeleteAsync(queue, ifUnused, ifEmpty, false, cancellationToken);
        _logger.LogDebug("Deleted queue '{Queue}'", queue);
    }

    public async Task DeleteExchangeAsync(string exchange, bool ifUnused = false,
        CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.ExchangeDeleteAsync(exchange, ifUnused, false, cancellationToken);
        _logger.LogDebug("Deleted exchange '{Exchange}'", exchange);
    }

    public async Task PurgeQueueAsync(string queue, CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.QueuePurgeAsync(queue, cancellationToken);
        _logger.LogDebug("Purged queue '{Queue}'", queue);
    }

    public async Task<uint> GetMessageCountAsync(string queue, CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        var result = await channel.QueueDeclarePassiveAsync(queue, cancellationToken);
        return result.MessageCount;
    }
}
