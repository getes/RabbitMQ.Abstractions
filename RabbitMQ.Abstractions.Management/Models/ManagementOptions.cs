namespace RabbitMQ.Abstractions.Management.Models;

public class ManagementOptions
{
    public string BaseUrl { get; set; } = "http://localhost:15672";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string DefaultVirtualHost { get; set; } = "/";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
