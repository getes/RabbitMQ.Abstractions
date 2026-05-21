using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Abstractions.Management.Implementation;
using RabbitMQ.Abstractions.Management.Interfaces;
using RabbitMQ.Abstractions.Management.Models;

namespace RabbitMQ.Abstractions.Management.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqManagement(this IServiceCollection services, Action<ManagementOptions> configure)
    {
        services.Configure(configure);

        services.AddHttpClient<IRabbitMqManagement, RabbitMqManagementClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ManagementOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
            client.Timeout = options.Timeout;

            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{options.UserName}:{options.Password}"));

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}
