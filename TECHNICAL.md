# Technical Documentation

## Architecture Overview

RabbitMQ.Abstractions follows the **Facade pattern** to hide RabbitMQ complexity behind a set of well-defined interfaces. The library is designed with **SOLID principles**, prioritizing interface segregation and dependency inversion.

## Project Structure

```
RabbitMQ.Abstractions/
├── RabbitMQ.Abstractions/
│   ├── Interfaces/
│   │   ├── IRabbitMqClient.cs          # Main facade (aggregates all operations)
│   │   ├── IMessagePublisher.cs        # Publish/send messages
│   │   ├── IMessageConsumer.cs         # Subscribe/consume messages
│   │   └── IQueueManager.cs            # Queue/exchange CRUD operations
│   ├── Models/
│   │   ├── RabbitMqOptions.cs          # Connection configuration
│   │   ├── MessageEnvelope.cs          # Message wrapper (headers, correlation, timestamp)
│   │   └── ConsumerOptions.cs          # Consumer behavior settings
│   ├── Implementation/
│   │   ├── RabbitMqClient.cs           # Facade implementation
│   │   ├── MessagePublisher.cs         # Publisher logic
│   │   ├── MessageConsumer.cs          # Consumer logic
│   │   └── QueueManager.cs             # Queue/exchange management
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs  # DI registration
│   ├── Serialization/
│   │   └── IMessageSerializer.cs       # Pluggable serialization contract
│   └── RabbitMQ.Abstractions.csproj
├── tests/
│   └── RabbitMQ.Abstractions.Tests/
│       └── RabbitMQ.Abstractions.Tests.csproj
├── .gitignore
├── RabbitMQ.Abstractions.sln
├── README.md
└── TECHNICAL.md
```

## Design Patterns

### Facade Pattern
`IRabbitMqClient` serves as the single entry point. Consumers of the library only need to inject one interface to access all operations, though they can also inject specific interfaces (`IMessagePublisher`, `IMessageConsumer`, `IQueueManager`) for more granular control.

### Interface Segregation (ISP)
Operations are split into focused interfaces:
- **IMessagePublisher** — Publishing only
- **IMessageConsumer** — Subscribing only
- **IQueueManager** — Infrastructure management only

This allows consumers to depend only on what they use.

### Strategy Pattern
`IMessageSerializer` allows swapping serialization strategies without changing the core library. Default implementation uses `System.Text.Json`, but consumers can provide their own (MessagePack, Protobuf, etc.).

### Options Pattern
Configuration is handled via `Microsoft.Extensions.Options` with `IOptions<RabbitMqOptions>`, following the standard .NET configuration pattern.

## Interface Contracts

### IRabbitMqClient (Facade)

```csharp
public interface IRabbitMqClient : IMessagePublisher, IMessageConsumer, IQueueManager, IAsyncDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
```

### IMessagePublisher

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message, 
        CancellationToken cancellationToken = default) where T : class;
    
    Task PublishAsync<T>(string exchange, string routingKey, T message, 
        MessageProperties properties, CancellationToken cancellationToken = default) where T : class;
    
    Task PublishBatchAsync<T>(string exchange, string routingKey, IEnumerable<T> messages, 
        CancellationToken cancellationToken = default) where T : class;
}
```

### IMessageConsumer

```csharp
public interface IMessageConsumer
{
    Task SubscribeAsync<T>(string queue, Func<MessageEnvelope<T>, Task> handler,
        ConsumerOptions? options = null, CancellationToken cancellationToken = default) where T : class;
    
    Task UnsubscribeAsync(string queue, CancellationToken cancellationToken = default);
    
    Task<MessageEnvelope<T>?> GetMessageAsync<T>(string queue,
        CancellationToken cancellationToken = default) where T : class;
}
```

### IQueueManager

```csharp
public interface IQueueManager
{
    Task DeclareQueueAsync(string queue, bool durable = true, bool exclusive = false,
        bool autoDelete = false, IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    Task DeclareExchangeAsync(string exchange, string type, bool durable = true,
        bool autoDelete = false, IDictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default);
    
    Task BindQueueAsync(string queue, string exchange, string routingKey,
        CancellationToken cancellationToken = default);
    
    Task DeleteQueueAsync(string queue, CancellationToken cancellationToken = default);
    
    Task DeleteExchangeAsync(string exchange, CancellationToken cancellationToken = default);
    
    Task PurgeQueueAsync(string queue, CancellationToken cancellationToken = default);
}
```

## Models

### RabbitMqOptions

```csharp
public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public ushort PrefetchCount { get; set; } = 10;
}
```

### MessageEnvelope\<T\>

```csharp
public class MessageEnvelope<T> where T : class
{
    public T Body { get; set; }
    public string MessageId { get; set; }
    public string CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public IDictionary<string, object> Headers { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
    public ulong DeliveryTag { get; set; }
}
```

### ConsumerOptions

```csharp
public class ConsumerOptions
{
    public bool AutoAck { get; set; } = false;
    public ushort PrefetchCount { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
```

## Dependency Injection Registration

```csharp
services.AddRabbitMq(options =>
{
    options.HostName = "rabbitmq-server";
    options.Port = 5672;
    options.UserName = "app_user";
    options.Password = "app_password";
});
```

This registers:
- `IRabbitMqClient` as Singleton
- `IMessagePublisher` as Singleton (same instance)
- `IMessageConsumer` as Singleton (same instance)
- `IQueueManager` as Singleton (same instance)
- `IMessageSerializer` as Singleton (default: JSON)

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| RabbitMQ.Client | 7.2.1 | Official .NET RabbitMQ client |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.x | DI registration |
| Microsoft.Extensions.Options | 10.x | Options pattern |
| Microsoft.Extensions.Logging.Abstractions | 10.x | Structured logging |

## Connection Management

The library manages a single persistent connection with automatic recovery:
- Connection is lazily initialized on first operation
- Automatic reconnection on network failures
- Channel pooling for concurrent operations
- Graceful shutdown via `IAsyncDisposable`

## Serialization

Default serializer uses `System.Text.Json` with these settings:
- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`

To provide a custom serializer:

```csharp
services.AddRabbitMq(options => { ... })
    .WithSerializer<MyCustomSerializer>();
```

## Future Considerations (NuGet Package)

The `.csproj` is pre-configured with NuGet metadata for future packaging:
- `PackageId`: RabbitMQ.Abstractions
- `Version`: follows SemVer
- `PackageTags`: rabbitmq, messaging, queue, abstraction
- SourceLink enabled for debugging

## Git Workflow

- **master** — Production-ready releases only
- **develop** — Integration branch, all feature work merges here
- **feature/*** — Created from `develop`, merged back to `develop` via PR

**Rule**: Never merge `develop` → `master` without explicit authorization.
