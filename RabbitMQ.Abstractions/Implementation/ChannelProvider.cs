using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Abstractions.Models;
using RabbitMQ.Client;

namespace RabbitMQ.Abstractions.Implementation;

internal sealed class ChannelProvider : IChannelProvider, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ChannelProvider> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public bool IsConnected => _connection is { IsOpen: true } && _channel is { IsOpen: true };

    public ChannelProvider(IOptions<RabbitMqOptions> options, ILogger<ChannelProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return _channel!;

        await ConnectAsync(cancellationToken);
        return _channel!;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled,
                RequestedHeartbeat = _options.RequestedHeartbeat,
                NetworkRecoveryInterval = _options.NetworkRecoveryInterval
            };

            _connection = await factory.CreateConnectionAsync(
                _options.ClientProvidedName ?? "RabbitMQ.Abstractions",
                cancellationToken);

            _channel = await _connection.CreateChannelAsync(
                new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _options.PrefetchCount,
                global: false,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}/{VHost}",
                _options.HostName, _options.Port, _options.VirtualHost);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                await _channel.CloseAsync(cancellationToken);
            }

            if (_connection is { IsOpen: true })
            {
                await _connection.CloseAsync(cancellationToken);
            }

            _logger.LogInformation("Disconnected from RabbitMQ");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();

        _channel?.Dispose();
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}
