namespace RabbitMQ.Abstractions.Management.Tests;

public class ManagementApiTests : IClassFixture<ManagementTestFixture>
{
    private readonly ManagementTestFixture _fixture;

    public ManagementApiTests(ManagementTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetOverview_ShouldReturnBrokerInfo()
    {
        var overview = await _fixture.Management.GetOverviewAsync();

        Assert.NotEmpty(overview.RabbitMqVersion);
        Assert.NotEmpty(overview.ErlangVersion);
        Assert.NotEmpty(overview.ClusterName);
    }

    [Fact]
    public async Task ListQueues_ShouldReturnQueues()
    {
        var queues = await _fixture.Management.ListQueuesAsync();

        Assert.NotNull(queues);
        // RabbitMQ always has at least the default vhost
    }

    [Fact]
    public async Task ListQueues_WithVhost_ShouldFilterByVhost()
    {
        var queues = await _fixture.Management.ListQueuesAsync("/");

        Assert.NotNull(queues);
        Assert.All(queues, q => Assert.Equal("/", q.VirtualHost));
    }

    [Fact]
    public async Task ListExchanges_ShouldReturnDefaultExchanges()
    {
        var exchanges = await _fixture.Management.ListExchangesAsync();

        Assert.NotNull(exchanges);
        // RabbitMQ always has default exchanges (amq.direct, amq.fanout, amq.topic, etc.)
        Assert.NotEmpty(exchanges);
    }

    [Fact]
    public async Task ListBindings_ShouldReturnBindings()
    {
        var bindings = await _fixture.Management.ListBindingsAsync();

        Assert.NotNull(bindings);
    }

    [Fact]
    public async Task ListConnections_ShouldReturnConnections()
    {
        var connections = await _fixture.Management.ListConnectionsAsync();

        Assert.NotNull(connections);
    }

    [Fact]
    public async Task ListVirtualHosts_ShouldReturnAtLeastDefaultVhost()
    {
        var vhosts = await _fixture.Management.ListVirtualHostsAsync();

        Assert.NotNull(vhosts);
        Assert.NotEmpty(vhosts);
        Assert.Contains(vhosts, v => v.Name == "/");
    }

    [Fact]
    public async Task ListConsumers_ShouldReturnConsumerList()
    {
        var consumers = await _fixture.Management.ListConsumersAsync();

        Assert.NotNull(consumers);
    }

    [Fact]
    public async Task GetQueue_NonExistent_ShouldReturnNull()
    {
        var queue = await _fixture.Management.GetQueueAsync("non-existent-queue-xyz");

        Assert.Null(queue);
    }

    [Fact]
    public async Task GetExchange_DefaultDirect_ShouldReturnInfo()
    {
        var exchange = await _fixture.Management.GetExchangeAsync("amq.direct");

        Assert.NotNull(exchange);
        Assert.Equal("amq.direct", exchange.Name);
        Assert.Equal("direct", exchange.Type);
        Assert.True(exchange.Durable);
    }
}
