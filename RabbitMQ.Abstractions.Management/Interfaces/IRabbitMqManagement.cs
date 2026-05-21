using RabbitMQ.Abstractions.Management.Models;

namespace RabbitMQ.Abstractions.Management.Interfaces;

public interface IRabbitMqManagement
{
    // Broker
    Task<BrokerOverview> GetOverviewAsync(CancellationToken cancellationToken = default);

    // Queues
    Task<IReadOnlyList<QueueInfo>> ListQueuesAsync(string? vhost = null, CancellationToken cancellationToken = default);
    Task<QueueInfo?> GetQueueAsync(string queue, string? vhost = null, CancellationToken cancellationToken = default);

    // Exchanges
    Task<IReadOnlyList<ExchangeInfo>> ListExchangesAsync(string? vhost = null, CancellationToken cancellationToken = default);
    Task<ExchangeInfo?> GetExchangeAsync(string exchange, string? vhost = null, CancellationToken cancellationToken = default);

    // Bindings
    Task<IReadOnlyList<BindingInfo>> ListBindingsAsync(string? vhost = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BindingInfo>> GetQueueBindingsAsync(string queue, string? vhost = null, CancellationToken cancellationToken = default);

    // Connections
    Task<IReadOnlyList<ConnectionInfo>> ListConnectionsAsync(CancellationToken cancellationToken = default);

    // Consumers
    Task<IReadOnlyList<ConsumerInfo>> ListConsumersAsync(string? vhost = null, CancellationToken cancellationToken = default);

    // Virtual Hosts
    Task<IReadOnlyList<VirtualHostInfo>> ListVirtualHostsAsync(CancellationToken cancellationToken = default);
}
