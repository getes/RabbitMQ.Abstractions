namespace RabbitMQ.Abstractions.Interfaces;

public interface IRabbitMqClient : IMessagePublisher, IMessageConsumer, IQueueManager, IAsyncDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
