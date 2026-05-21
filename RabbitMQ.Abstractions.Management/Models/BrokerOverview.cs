using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class BrokerOverview
{
    [JsonPropertyName("management_version")]
    public string ManagementVersion { get; set; } = string.Empty;

    [JsonPropertyName("rabbitmq_version")]
    public string RabbitMqVersion { get; set; } = string.Empty;

    [JsonPropertyName("erlang_version")]
    public string ErlangVersion { get; set; } = string.Empty;

    [JsonPropertyName("cluster_name")]
    public string ClusterName { get; set; } = string.Empty;

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    [JsonPropertyName("object_totals")]
    public ObjectTotals ObjectTotals { get; set; } = new();

    [JsonPropertyName("queue_totals")]
    public QueueTotals QueueTotals { get; set; } = new();
}

public class ObjectTotals
{
    [JsonPropertyName("connections")]
    public int Connections { get; set; }

    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [JsonPropertyName("exchanges")]
    public int Exchanges { get; set; }

    [JsonPropertyName("queues")]
    public int Queues { get; set; }

    [JsonPropertyName("consumers")]
    public int Consumers { get; set; }
}

public class QueueTotals
{
    [JsonPropertyName("messages")]
    public uint Messages { get; set; }

    [JsonPropertyName("messages_ready")]
    public uint MessagesReady { get; set; }

    [JsonPropertyName("messages_unacknowledged")]
    public uint MessagesUnacknowledged { get; set; }
}
