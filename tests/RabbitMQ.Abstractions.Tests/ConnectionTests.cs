namespace RabbitMQ.Abstractions.Tests;

public class ConnectionTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public ConnectionTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Client_ShouldBeConnected()
    {
        Assert.True(_fixture.Client.IsConnected);
    }

    [Fact]
    public async Task Client_ShouldReconnectAfterDisconnect()
    {
        await _fixture.Client.DisconnectAsync();
        Assert.False(_fixture.Client.IsConnected);

        await _fixture.Client.ConnectAsync();
        Assert.True(_fixture.Client.IsConnected);
    }
}
