using Microsoft.Extensions.Logging;
using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Abstractions.Models;
using RabbitMQ.Abstractions.Serialization;
using RabbitMQ.Client;

namespace RabbitMQ.Abstractions.Implementation;

internal sealed class MessagePublisher : IMessagePublisher
{
    private readonly IChannelProvider _channelProvider;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<MessagePublisher> _logger;

    public MessagePublisher(IChannelProvider channelProvider, IMessageSerializer serializer, ILogger<MessagePublisher> logger)
    {
        _channelProvider = channelProvider;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message,
        CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(exchange, routingKey, message, new MessageProperties(), cancellationToken);
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message,
        MessageProperties properties, CancellationToken cancellationToken = default) where T : class
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        var body = _serializer.Serialize(message);
        var basicProperties = CreateBasicProperties(properties);

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: basicProperties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Published message to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
    }

    public async Task PublishBatchAsync<T>(string exchange, string routingKey, IEnumerable<T> messages,
        CancellationToken cancellationToken = default) where T : class
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        var properties = new BasicProperties
        {
            ContentType = _serializer.ContentType,
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var body = _serializer.Serialize(message);
            properties.MessageId = Guid.NewGuid().ToString();

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }

        _logger.LogDebug("Published batch to exchange '{Exchange}' with routing key '{RoutingKey}'", exchange, routingKey);
    }

    private BasicProperties CreateBasicProperties(MessageProperties properties)
    {
        var basicProps = new BasicProperties
        {
            ContentType = properties.ContentType ?? _serializer.ContentType,
            ContentEncoding = properties.ContentEncoding,
            DeliveryMode = properties.DeliveryMode.HasValue
                ? (DeliveryModes)properties.DeliveryMode.Value
                : DeliveryModes.Persistent,
            Priority = properties.Priority ?? 0,
            MessageId = properties.MessageId ?? Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            CorrelationId = properties.CorrelationId,
            ReplyTo = properties.ReplyTo,
            Expiration = properties.Expiration,
            Type = properties.Type,
            AppId = properties.AppId
        };

        if (properties.Headers.Count > 0)
        {
            basicProps.Headers = new Dictionary<string, object?>(properties.Headers);
        }

        return basicProps;
    }
}
