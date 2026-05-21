using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class ExchangeInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("durable")]
    public bool Durable { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool AutoDelete { get; set; }

    [JsonPropertyName("internal")]
    public bool Internal { get; set; }
}
