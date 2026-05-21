using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Abstractions.Models;
using RabbitMQ.Abstractions.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Abstractions.Implementation;

internal sealed class MessageConsumer : IMessageConsumer
{
    private readonly IChannelProvider _channelProvider;
    private readonly IMessageSerializer _serializer;
    private readonly ILogger<MessageConsumer> _logger;
    private readonly ConcurrentDictionary<string, string> _consumerTags = new();

    public MessageConsumer(IChannelProvider channelProvider, IMessageSerializer serializer, ILogger<MessageConsumer> logger)
    {
        _channelProvider = channelProvider;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task SubscribeAsync<T>(string queue, Func<MessageEnvelope<T>, Task> handler,
        ConsumerOptions? options = null, CancellationToken cancellationToken = default) where T : class
    {
        options ??= new ConsumerOptions();
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);

        if (options.PrefetchCount > 0)
        {
            await channel.BasicQosAsync(0, options.PrefetchCount, false, cancellationToken);
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = _serializer.Deserialize<T>(ea.Body.Span);
                if (body is null)
                {
                    _logger.LogWarning("Failed to deserialize message from queue '{Queue}'", queue);
                    if (!options.AutoAck)
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                    return;
                }

                var envelope = new MessageEnvelope<T>
                {
                    Body = body,
                    MessageId = ea.BasicProperties.MessageId,
                    CorrelationId = ea.BasicProperties.CorrelationId,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(ea.BasicProperties.Timestamp.UnixTime).UtcDateTime,
                    Headers = ea.BasicProperties.Headers?.ToDictionary(
                        k => k.Key,
                        v => (object?)v.Value) ?? new Dictionary<string, object?>(),
                    Exchange = ea.Exchange,
                    RoutingKey = ea.RoutingKey,
                    DeliveryTag = ea.DeliveryTag,
                    Redelivered = ea.Redelivered
                };

                await ExecuteWithRetry(handler, envelope, options.RetryCount, options.RetryDelay);

                if (!options.AutoAck)
                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue '{Queue}'", queue);
                if (!options.AutoAck)
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: queue,
            autoAck: options.AutoAck,
            consumerTag: options.ConsumerTag ?? string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _consumerTags[queue] = consumerTag;
        _logger.LogInformation("Subscribed to queue '{Queue}' with consumer tag '{ConsumerTag}'", queue, consumerTag);
    }

    public async Task UnsubscribeAsync(string queue, CancellationToken cancellationToken = default)
    {
        if (_consumerTags.TryRemove(queue, out var consumerTag))
        {
            var channel = await _channelProvider.GetChannelAsync(cancellationToken);
            await channel.BasicCancelAsync(consumerTag, false, cancellationToken);
            _logger.LogInformation("Unsubscribed from queue '{Queue}'", queue);
        }
    }

    public async Task<MessageEnvelope<T>?> GetMessageAsync<T>(string queue, bool autoAck = false,
        CancellationToken cancellationToken = default) where T : class
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        var result = await channel.BasicGetAsync(queue, autoAck, cancellationToken);

        if (result is null)
            return null;

        var body = _serializer.Deserialize<T>(result.Body.Span);
        if (body is null)
            return null;

        return new MessageEnvelope<T>
        {
            Body = body,
            MessageId = result.BasicProperties.MessageId,
            CorrelationId = result.BasicProperties.CorrelationId,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(result.BasicProperties.Timestamp.UnixTime).UtcDateTime,
            Headers = result.BasicProperties.Headers?.ToDictionary(
                k => k.Key,
                v => (object?)v.Value) ?? new Dictionary<string, object?>(),
            Exchange = result.Exchange,
            RoutingKey = result.RoutingKey,
            DeliveryTag = result.DeliveryTag,
            Redelivered = result.Redelivered
        };
    }

    public async Task AckAsync(ulong deliveryTag, CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.BasicAckAsync(deliveryTag, false, cancellationToken);
    }

    public async Task NackAsync(ulong deliveryTag, bool requeue = true, CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync(cancellationToken);
        await channel.BasicNackAsync(deliveryTag, false, requeue, cancellationToken);
    }

    private static async Task ExecuteWithRetry<T>(Func<MessageEnvelope<T>, Task> handler,
        MessageEnvelope<T> envelope, int retryCount, TimeSpan retryDelay) where T : class
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                await handler(envelope);
                return;
            }
            catch when (++attempts <= retryCount)
            {
                await Task.Delay(retryDelay * attempts);
            }
        }
    }
}
