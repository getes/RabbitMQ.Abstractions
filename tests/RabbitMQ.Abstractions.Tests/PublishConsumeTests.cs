using RabbitMQ.Abstractions.Models;

namespace RabbitMQ.Abstractions.Tests;

public class PublishConsumeTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    private readonly string _testQueue;
    private readonly string _testExchange;

    public PublishConsumeTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _testQueue = $"test-pubsub-{Guid.NewGuid():N}";
        _testExchange = $"test-pubsub-ex-{Guid.NewGuid():N}";
    }

    public async Task InitializeAsync()
    {
        await _fixture.QueueManager.DeclareExchangeAsync(_testExchange, "topic", durable: false);
        await _fixture.QueueManager.DeclareQueueAsync(_testQueue, durable: false);
        await _fixture.QueueManager.BindQueueAsync(_testQueue, _testExchange, "test.#");
    }

    public async Task DisposeAsync()
    {
        try { await _fixture.Consumer.UnsubscribeAsync(_testQueue); } catch { }
        try { await _fixture.QueueManager.DeleteQueueAsync(_testQueue); } catch { }
        try { await _fixture.QueueManager.DeleteExchangeAsync(_testExchange); } catch { }
    }

    [Fact]
    public async Task PublishAndGet_ShouldRoundTripMessage()
    {
        var message = new TestMessage { Id = 1, Content = "Hello RabbitMQ" };

        await _fixture.Publisher.PublishAsync(_testExchange, "test.created", message);

        // Small delay to allow message to be routed
        await Task.Delay(100);

        var envelope = await _fixture.Consumer.GetMessageAsync<TestMessage>(_testQueue, autoAck: true);

        Assert.NotNull(envelope);
        Assert.Equal(1, envelope.Body.Id);
        Assert.Equal("Hello RabbitMQ", envelope.Body.Content);
        Assert.Equal(_testExchange, envelope.Exchange);
        Assert.Equal("test.created", envelope.RoutingKey);
    }

    [Fact]
    public async Task PublishAndGet_WithProperties_ShouldPreserveCorrelationId()
    {
        var message = new TestMessage { Id = 2, Content = "Correlated" };
        var properties = new MessageProperties
        {
            CorrelationId = "corr-123",
            Priority = 3
        };

        await _fixture.Publisher.PublishAsync(_testExchange, "test.updated", message, properties);
        await Task.Delay(100);

        var envelope = await _fixture.Consumer.GetMessageAsync<TestMessage>(_testQueue, autoAck: true);

        Assert.NotNull(envelope);
        Assert.Equal("corr-123", envelope.CorrelationId);
        Assert.Equal(2, envelope.Body.Id);
    }

    [Fact]
    public async Task PublishAndSubscribe_ShouldDeliverToHandler()
    {
        var received = new TaskCompletionSource<MessageEnvelope<TestMessage>>();

        await _fixture.Consumer.SubscribeAsync<TestMessage>(_testQueue, async (envelope) =>
        {
            received.TrySetResult(envelope);
            await Task.CompletedTask;
        }, new ConsumerOptions { AutoAck = true });

        var message = new TestMessage { Id = 3, Content = "Subscribed" };
        await _fixture.Publisher.PublishAsync(_testExchange, "test.subscribed", message);

        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(3, result.Body.Id);
        Assert.Equal("Subscribed", result.Body.Content);
    }

    [Fact]
    public async Task PublishBatch_ShouldDeliverAllMessages()
    {
        var messages = Enumerable.Range(1, 5)
            .Select(i => new TestMessage { Id = i, Content = $"Batch-{i}" })
            .ToList();

        await _fixture.Publisher.PublishBatchAsync(_testExchange, "test.batch", messages);
        await Task.Delay(200);

        var count = await _fixture.QueueManager.GetMessageCountAsync(_testQueue);
        Assert.Equal(5u, count);
    }

    [Fact]
    public async Task ManualAck_ShouldRemoveMessageFromQueue()
    {
        var message = new TestMessage { Id = 4, Content = "AckMe" };
        await _fixture.Publisher.PublishAsync(_testExchange, "test.ack", message);
        await Task.Delay(100);

        var envelope = await _fixture.Consumer.GetMessageAsync<TestMessage>(_testQueue, autoAck: false);
        Assert.NotNull(envelope);

        await _fixture.Consumer.AckAsync(envelope.DeliveryTag);

        var count = await _fixture.QueueManager.GetMessageCountAsync(_testQueue);
        Assert.Equal(0u, count);
    }

    [Fact]
    public async Task Nack_WithRequeue_ShouldReturnMessageToQueue()
    {
        var message = new TestMessage { Id = 5, Content = "NackMe" };
        await _fixture.Publisher.PublishAsync(_testExchange, "test.nack", message);
        await Task.Delay(100);

        var envelope = await _fixture.Consumer.GetMessageAsync<TestMessage>(_testQueue, autoAck: false);
        Assert.NotNull(envelope);

        await _fixture.Consumer.NackAsync(envelope.DeliveryTag, requeue: true);
        await Task.Delay(100);

        var count = await _fixture.QueueManager.GetMessageCountAsync(_testQueue);
        Assert.Equal(1u, count);
    }
}

public class TestMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
}
