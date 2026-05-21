using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class ConnectionInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("peer_host")]
    public string PeerHost { get; set; } = string.Empty;

    [JsonPropertyName("peer_port")]
    public int PeerPort { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("client_properties")]
    public Dictionary<string, object?>? ClientProperties { get; set; }

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;
}
