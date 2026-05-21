using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Abstractions.Management.Extensions;
using RabbitMQ.Abstractions.Management.Interfaces;

namespace RabbitMQ.Abstractions.Management.Tests;

public class ManagementTestFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IRabbitMqManagement Management => ServiceProvider.GetRequiredService<IRabbitMqManagement>();

    public Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddRabbitMqManagement(options =>
        {
            options.BaseUrl = "http://localhost:15672";
            options.UserName = "guest";
            options.Password = "guest";
            options.DefaultVirtualHost = "/";
        });

        ServiceProvider = services.BuildServiceProvider();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
        return Task.CompletedTask;
    }
}
