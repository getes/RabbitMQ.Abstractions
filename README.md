# RabbitMQ.Abstractions

A .NET 9 library that provides a clean, dependency-injection-friendly abstraction over RabbitMQ. Import the library, register the service, and start publishing/consuming messages without dealing with RabbitMQ internals.

## Features

- Simple facade over RabbitMQ operations (publish, consume, queue management)
- Built for dependency injection (`IServiceCollection` extension)
- Pluggable serialization (defaults to `System.Text.Json`)
- Async-first API
- Strongly-typed message handling
- Automatic connection and channel management with recovery

## Installation

### From DLL reference

Add a reference to `RabbitMQ.Abstractions.dll` in your project.

### From NuGet (future)

```bash
dotnet add package RabbitMQ.Abstractions
```

## Getting Started

### How it works

This library uses **Dependency Injection** (Microsoft.Extensions.DependencyInjection) to provide RabbitMQ functionality. You register it once in your application's service configuration, and then inject the interfaces wherever you need them.

The library handles connection lifecycle, channel management, serialization, and recovery automatically — you only interact with clean interfaces.

### Step 1: Add the using statement

```csharp
using RabbitMQ.Abstractions.Extensions;
```

### Step 2: Register the service in your DI container

In your `Program.cs` (or `Startup.cs` for older project styles), register the RabbitMQ services with your connection configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register RabbitMQ with connection parameters
builder.Services.AddRabbitMq(options =>
{
    options.HostName = "your-rabbitmq-server";
    options.Port = 5672;
    options.UserName = "your-username";
    options.Password = "your-password";
    options.VirtualHost = "/";
});

var app = builder.Build();
```

This single call registers all the following interfaces as **singletons** in the DI container:

| Interface | Purpose |
|-----------|---------|
| `IRabbitMqClient` | Full facade — publish, consume, and manage queues from one interface |
| `IMessagePublisher` | Publishing messages only |
| `IMessageConsumer` | Consuming/subscribing to messages only |
| `IQueueManager` | Queue and exchange management only |

You can inject whichever interface fits your class's responsibility (Interface Segregation Principle).

### Step 3: Inject and use

Inject any of the registered interfaces via constructor injection:

```csharp
using RabbitMQ.Abstractions.Interfaces;

public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PlaceOrderAsync(Order order)
    {
        await _publisher.PublishAsync("orders-exchange", "order.created", order);
    }
}
```

The connection to RabbitMQ is established lazily on the first operation — no need to manually connect or manage the connection.

## Usage Examples

### Publishing messages

```csharp
// Simple publish (auto-serialized to JSON)
await _publisher.PublishAsync("my-exchange", "routing.key", myObject);

// Publish with custom properties
await _publisher.PublishAsync("my-exchange", "routing.key", myObject, new MessageProperties
{
    CorrelationId = "abc-123",
    Priority = 5,
    Expiration = "60000" // TTL in ms
});

// Batch publish
await _publisher.PublishBatchAsync("my-exchange", "routing.key", listOfMessages);
```

### Consuming messages (subscription)

```csharp
using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Abstractions.Models;

public class OrderConsumerService
{
    private readonly IMessageConsumer _consumer;

    public OrderConsumerService(IMessageConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _consumer.SubscribeAsync<Order>("orders-queue", async (envelope) =>
        {
            // envelope.Body contains your deserialized Order object
            Console.WriteLine($"Received order: {envelope.Body.Id}");
            Console.WriteLine($"Message ID: {envelope.MessageId}");
            Console.WriteLine($"Timestamp: {envelope.Timestamp}");
        },
        new ConsumerOptions
        {
            AutoAck = false,       // Manual acknowledgment (default)
            PrefetchCount = 10,    // How many messages to prefetch
            RetryCount = 3,        // Retry handler on failure
            RetryDelay = TimeSpan.FromSeconds(1)
        },
        cancellationToken);
    }

    public async Task StopAsync()
    {
        await _consumer.UnsubscribeAsync("orders-queue");
    }
}
```

### Getting a single message (pull)

```csharp
var envelope = await _consumer.GetMessageAsync<Order>("orders-queue");
if (envelope != null)
{
    // Process the message
    ProcessOrder(envelope.Body);

    // Manually acknowledge
    await _consumer.AckAsync(envelope.DeliveryTag);
}
```

### Managing queues and exchanges

```csharp
using RabbitMQ.Abstractions.Interfaces;

public class InfrastructureSetup
{
    private readonly IQueueManager _queueManager;

    public InfrastructureSetup(IQueueManager queueManager)
    {
        _queueManager = queueManager;
    }

    public async Task InitializeAsync()
    {
        // Declare an exchange
        await _queueManager.DeclareExchangeAsync("orders-exchange", "topic", durable: true);

        // Declare a queue
        await _queueManager.DeclareQueueAsync("orders-queue", durable: true);

        // Bind queue to exchange with routing key pattern
        await _queueManager.BindQueueAsync("orders-queue", "orders-exchange", "order.*");

        // Check message count
        uint count = await _queueManager.GetMessageCountAsync("orders-queue");
    }
}
```

### Using the full facade

If your class needs multiple operations, inject `IRabbitMqClient` which combines all interfaces:

```csharp
public class MessagingService
{
    private readonly IRabbitMqClient _client;

    public MessagingService(IRabbitMqClient client)
    {
        _client = client;
    }

    public async Task SetupAndPublishAsync()
    {
        // Queue management
        await _client.DeclareExchangeAsync("events", "fanout");
        await _client.DeclareQueueAsync("events-queue");
        await _client.BindQueueAsync("events-queue", "events", "");

        // Publish
        await _client.PublishAsync("events", "", new { Type = "UserCreated", UserId = 42 });

        // Subscribe
        await _client.SubscribeAsync<dynamic>("events-queue", async (msg) =>
        {
            Console.WriteLine($"Event received: {msg.Body}");
        });
    }
}
```

## Custom Serialization

By default, messages are serialized using `System.Text.Json` (camelCase, null values ignored). To use a different serializer:

```csharp
builder.Services.AddRabbitMq(options => { /* ... */ })
    .WithSerializer<MyCustomSerializer>();
```

Your custom serializer must implement `IMessageSerializer`:

```csharp
using RabbitMQ.Abstractions.Serialization;

public class MessagePackSerializer : IMessageSerializer
{
    public string ContentType => "application/x-msgpack";

    public byte[] Serialize<T>(T message) where T : class
    {
        // Your serialization logic
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> data) where T : class
    {
        // Your deserialization logic
    }
}
```

## Configuration Reference

All configuration is provided through `RabbitMqOptions` in the `AddRabbitMq` call:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HostName` | `string` | `"localhost"` | RabbitMQ server hostname |
| `Port` | `int` | `5672` | RabbitMQ server port |
| `UserName` | `string` | `"guest"` | Connection username |
| `Password` | `string` | `"guest"` | Connection password |
| `VirtualHost` | `string` | `"/"` | RabbitMQ virtual host |
| `AutomaticRecoveryEnabled` | `bool` | `true` | Auto-reconnect on connection failure |
| `RequestedHeartbeat` | `TimeSpan` | `60s` | Connection heartbeat interval |
| `NetworkRecoveryInterval` | `TimeSpan` | `5s` | Delay between reconnection attempts |
| `PrefetchCount` | `ushort` | `10` | Default prefetch count for consumers |
| `ClientProvidedName` | `string?` | `null` | Application name shown in RabbitMQ management UI |

### Configuration from appsettings.json

You can also load configuration from your app settings:

```json
{
  "RabbitMq": {
    "HostName": "rabbitmq-server",
    "Port": 5672,
    "UserName": "app_user",
    "Password": "app_password",
    "VirtualHost": "/",
    "PrefetchCount": 20
  }
}
```

```csharp
var rabbitConfig = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddRabbitMq(options =>
{
    options.HostName = rabbitConfig["HostName"] ?? "localhost";
    options.Port = int.Parse(rabbitConfig["Port"] ?? "5672");
    options.UserName = rabbitConfig["UserName"] ?? "guest";
    options.Password = rabbitConfig["Password"] ?? "guest";
    options.VirtualHost = rabbitConfig["VirtualHost"] ?? "/";
});
```

## Requirements

- .NET 9.0 or later
- RabbitMQ server instance (3.x or later)
- Microsoft.Extensions.DependencyInjection (included in ASP.NET Core; add manually for console apps)

## License

MIT
