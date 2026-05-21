using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class VirtualHostInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public uint Messages { get; set; }

    [JsonPropertyName("messages_ready")]
    public uint MessagesReady { get; set; }

    [JsonPropertyName("messages_unacknowledged")]
    public uint MessagesUnacknowledged { get; set; }

    [JsonPropertyName("tracing")]
    public bool Tracing { get; set; }
}
