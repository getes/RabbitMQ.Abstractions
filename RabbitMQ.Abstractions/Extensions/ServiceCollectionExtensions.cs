using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Abstractions.Implementation;
using RabbitMQ.Abstractions.Interfaces;
using RabbitMQ.Abstractions.Models;
using RabbitMQ.Abstractions.Serialization;

namespace RabbitMQ.Abstractions.Extensions;

public static class ServiceCollectionExtensions
{
    public static RabbitMqBuilder AddRabbitMq(this IServiceCollection services, Action<RabbitMqOptions> configure)
    {
        services.Configure(configure);

        services.AddSingleton<IChannelProvider, ChannelProvider>();
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IMessageConsumer, MessageConsumer>();
        services.AddSingleton<IQueueManager, QueueManager>();
        services.AddSingleton<IRabbitMqClient, RabbitMqClient>();

        return new RabbitMqBuilder(services);
    }
}

public sealed class RabbitMqBuilder
{
    public IServiceCollection Services { get; }

    internal RabbitMqBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public RabbitMqBuilder WithSerializer<TSerializer>() where TSerializer : class, IMessageSerializer
    {
        Services.AddSingleton<IMessageSerializer, TSerializer>();
        return this;
    }
}
