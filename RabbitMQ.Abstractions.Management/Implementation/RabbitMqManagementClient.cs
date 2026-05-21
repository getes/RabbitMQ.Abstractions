using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Abstractions.Management.Interfaces;
using RabbitMQ.Abstractions.Management.Models;

namespace RabbitMQ.Abstractions.Management.Implementation;

internal sealed class RabbitMqManagementClient : IRabbitMqManagement
{
    private readonly HttpClient _httpClient;
    private readonly ManagementOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqManagementClient(HttpClient httpClient, IOptions<ManagementOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<BrokerOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<BrokerOverview>("/api/overview", cancellationToken);
        return result ?? new BrokerOverview();
    }

    public async Task<IReadOnlyList<QueueInfo>> ListQueuesAsync(string? vhost = null, CancellationToken cancellationToken = default)
    {
        var path = vhost is null
            ? "/api/queues"
            : $"/api/queues/{Uri.EscapeDataString(vhost)}";

        return await GetListAsync<QueueInfo>(path, cancellationToken);
    }

    public async Task<QueueInfo?> GetQueueAsync(string queue, string? vhost = null, CancellationToken cancellationToken = default)
    {
        var v = Uri.EscapeDataString(vhost ?? _options.DefaultVirtualHost);
        var q = Uri.EscapeDataString(queue);
        return await GetAsync<QueueInfo>($"/api/queues/{v}/{q}", cancellationToken);
    }

    public async Task<IReadOnlyList<ExchangeInfo>> ListExchangesAsync(string? vhost = null, CancellationToken cancellationToken = default)
    {
        var path = vhost is null
            ? "/api/exchanges"
            : $"/api/exchanges/{Uri.EscapeDataString(vhost)}";

        return await GetListAsync<ExchangeInfo>(path, cancellationToken);
    }

    public async Task<ExchangeInfo?> GetExchangeAsync(string exchange, string? vhost = null, CancellationToken cancellationToken = default)
    {
        var v = Uri.EscapeDataString(vhost ?? _options.DefaultVirtualHost);
        var e = Uri.EscapeDataString(exchange);
        return await GetAsync<ExchangeInfo>($"/api/exchanges/{v}/{e}", cancellationToken);
    }

    public async Task<IReadOnlyList<BindingInfo>> ListBindingsAsync(string? vhost = null, CancellationToken cancellationToken = default)
    {
        var path = vhost is null
            ? "/api/bindings"
            : $"/api/bindings/{Uri.EscapeDataString(vhost)}";

        return await GetListAsync<BindingInfo>(path, cancellationToken);
    }

    public async Task<IReadOnlyList<BindingInfo>> GetQueueBindingsAsync(string queue, string? vhost = null, CancellationToken cancellationToken = default)
    {
        var v = Uri.EscapeDataString(vhost ?? _options.DefaultVirtualHost);
        var q = Uri.EscapeDataString(queue);
        return await GetListAsync<BindingInfo>($"/api/queues/{v}/{q}/bindings", cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectionInfo>> ListConnectionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<ConnectionInfo>("/api/connections", cancellationToken);
    }

    public async Task<IReadOnlyList<ConsumerInfo>> ListConsumersAsync(string? vhost = null, CancellationToken cancellationToken = default)
    {
        var path = vhost is null
            ? "/api/consumers"
            : $"/api/consumers/{Uri.EscapeDataString(vhost)}";

        return await GetListAsync<ConsumerInfo>(path, cancellationToken);
    }

    public async Task<IReadOnlyList<VirtualHostInfo>> ListVirtualHostsAsync(CancellationToken cancellationToken = default)
    {
        return await GetListAsync<VirtualHostInfo>("/api/vhosts", cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken) where T : class
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken);
        return result ?? [];
    }
}
