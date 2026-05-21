using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class ConsumerInfo
{
    [JsonPropertyName("consumer_tag")]
    public string ConsumerTag { get; set; } = string.Empty;

    [JsonPropertyName("prefetch_count")]
    public int PrefetchCount { get; set; }

    [JsonPropertyName("ack_required")]
    public bool AckRequired { get; set; }

    [JsonPropertyName("exclusive")]
    public bool Exclusive { get; set; }

    [JsonPropertyName("queue")]
    public ConsumerQueueInfo Queue { get; set; } = new();

    [JsonPropertyName("channel_details")]
    public ChannelDetails ChannelDetails { get; set; } = new();
}

public class ConsumerQueueInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;
}

public class ChannelDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("connection_name")]
    public string ConnectionName { get; set; } = string.Empty;

    [JsonPropertyName("peer_host")]
    public string PeerHost { get; set; } = string.Empty;

    [JsonPropertyName("peer_port")]
    public int PeerPort { get; set; }
}
