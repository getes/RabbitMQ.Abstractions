namespace RabbitMQ.Abstractions.Models;

public class ConsumerOptions
{
    public bool AutoAck { get; set; }
    public ushort PrefetchCount { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public string? ConsumerTag { get; set; }
}
