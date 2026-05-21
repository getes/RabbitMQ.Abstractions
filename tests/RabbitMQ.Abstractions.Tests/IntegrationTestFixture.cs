using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Abstractions.Extensions;
using RabbitMQ.Abstractions.Interfaces;

namespace RabbitMQ.Abstractions.Tests;

public class IntegrationTestFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IRabbitMqClient Client => ServiceProvider.GetRequiredService<IRabbitMqClient>();
    public IMessagePublisher Publisher => ServiceProvider.GetRequiredService<IMessagePublisher>();
    public IMessageConsumer Consumer => ServiceProvider.GetRequiredService<IMessageConsumer>();
    public IQueueManager QueueManager => ServiceProvider.GetRequiredService<IQueueManager>();

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));

        services.AddRabbitMq(options =>
        {
            options.HostName = "localhost";
            options.Port = 5672;
            options.UserName = "guest";
            options.Password = "guest";
            options.VirtualHost = "/";
            options.PrefetchCount = 10;
            options.ClientProvidedName = "IntegrationTests";
        });

        ServiceProvider = services.BuildServiceProvider();

        await Client.ConnectAsync();
    }

    public async Task DisposeAsync()
    {
        await Client.DisposeAsync();

        if (ServiceProvider is IAsyncDisposable disposable)
            await disposable.DisposeAsync();
    }
}
