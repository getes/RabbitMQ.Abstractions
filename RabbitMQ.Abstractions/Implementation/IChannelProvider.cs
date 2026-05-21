using RabbitMQ.Client;

namespace RabbitMQ.Abstractions.Implementation;

internal interface IChannelProvider
{
    Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default);

    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
