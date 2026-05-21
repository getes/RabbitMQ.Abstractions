using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class QueueInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("durable")]
    public bool Durable { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool AutoDelete { get; set; }

    [JsonPropertyName("exclusive")]
    public bool Exclusive { get; set; }

    [JsonPropertyName("messages")]
    public uint Messages { get; set; }

    [JsonPropertyName("messages_ready")]
    public uint MessagesReady { get; set; }

    [JsonPropertyName("messages_unacknowledged")]
    public uint MessagesUnacknowledged { get; set; }

    [JsonPropertyName("consumers")]
    public int Consumers { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("memory")]
    public long Memory { get; set; }

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;
}
