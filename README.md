# RabbitMQ.Abstractions

A .NET 9 library that provides a clean, dependency-injection-friendly abstraction over RabbitMQ. Import the library, register the service, and start publishing/consuming messages without dealing with RabbitMQ internals.

## Features

- Simple facade over RabbitMQ operations (publish, consume, queue management)
- Built for dependency injection (`IServiceCollection` extension)
- Pluggable serialization (defaults to `System.Text.Json`)
- Async-first API
- Strongly-typed message handling
- Automatic connection/channel management

## Installation

### From DLL reference

Add a reference to `RabbitMQ.Abstractions.dll` in your project.

### From NuGet (future)

```bash
dotnet add package RabbitMQ.Abstractions
```

## Quick Start

### 1. Register the service

```csharp
services.AddRabbitMq(options =>
{
    options.HostName = "localhost";
    options.Port = 5672;
    options.UserName = "guest";
    options.Password = "guest";
    options.VirtualHost = "/";
});
```

### 2. Publish a message

```csharp
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

### 3. Consume messages

```csharp
public class OrderConsumerService
{
    private readonly IMessageConsumer _consumer;

    public OrderConsumerService(IMessageConsumer consumer)
    {
        _consumer = consumer;
    }

    public void Start()
    {
        _consumer.Subscribe<Order>("orders-queue", async (message) =>
        {
            // Process the order
            Console.WriteLine($"Received order: {message.Id}");
        });
    }
}
```

### 4. Manage queues and exchanges

```csharp
public class SetupService
{
    private readonly IQueueManager _queueManager;

    public SetupService(IQueueManager queueManager)
    {
        _queueManager = queueManager;
    }

    public async Task InitializeAsync()
    {
        await _queueManager.DeclareExchangeAsync("orders-exchange", ExchangeType.Topic);
        await _queueManager.DeclareQueueAsync("orders-queue", durable: true);
        await _queueManager.BindQueueAsync("orders-queue", "orders-exchange", "order.*");
    }
}
```

## Configuration

All configuration is provided through `RabbitMqOptions`:

| Property | Default | Description |
|----------|---------|-------------|
| `HostName` | `localhost` | RabbitMQ server hostname |
| `Port` | `5672` | RabbitMQ server port |
| `UserName` | `guest` | Connection username |
| `Password` | `guest` | Connection password |
| `VirtualHost` | `/` | RabbitMQ virtual host |
| `AutomaticRecoveryEnabled` | `true` | Auto-reconnect on failure |
| `RequestedHeartbeat` | `60s` | Connection heartbeat interval |

## Requirements

- .NET 9.0 or later
- RabbitMQ server instance (3.x or later)

## License

MIT
