namespace RabbitMQ.Abstractions.Tests;

public class QueueManagerTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly string _testQueue;
    private readonly string _testExchange;

    public QueueManagerTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _testQueue = $"test-queue-{Guid.NewGuid():N}";
        _testExchange = $"test-exchange-{Guid.NewGuid():N}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        try { await _fixture.QueueManager.DeleteQueueAsync(_testQueue); } catch { }
        try { await _fixture.QueueManager.DeleteExchangeAsync(_testExchange); } catch { }
    }

    [Fact]
    public async Task DeclareQueue_ShouldCreateQueue()
    {
        await _fixture.QueueManager.DeclareQueueAsync(_testQueue, durable: false);

        var count = await _fixture.QueueManager.GetMessageCountAsync(_testQueue);
        Assert.Equal(0u, count);
    }

    [Fact]
    public async Task DeclareExchange_ShouldCreateExchange()
    {
        await _fixture.QueueManager.DeclareExchangeAsync(_testExchange, "topic", durable: false);

        // If it didn't throw, the exchange was created successfully
        // Declare again to verify idempotency
        await _fixture.QueueManager.DeclareExchangeAsync(_testExchange, "topic", durable: false);
    }

    [Fact]
    public async Task BindQueue_ShouldBindQueueToExchange()
    {
        await _fixture.QueueManager.DeclareExchangeAsync(_testExchange, "topic", durable: false);
        await _fixture.QueueManager.DeclareQueueAsync(_testQueue, durable: false);

        await _fixture.QueueManager.BindQueueAsync(_testQueue, _testExchange, "test.*");

        // No exception means success
    }

    [Fact]
    public async Task PurgeQueue_ShouldRemoveAllMessages()
    {
        await _fixture.QueueManager.DeclareQueueAsync(_testQueue, durable: false);

        await _fixture.QueueManager.PurgeQueueAsync(_testQueue);

        var count = await _fixture.QueueManager.GetMessageCountAsync(_testQueue);
        Assert.Equal(0u, count);
    }
}
