namespace RabbitMQ.Abstractions.Models;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public ushort PrefetchCount { get; set; } = 10;
    public string? ClientProvidedName { get; set; }
}
