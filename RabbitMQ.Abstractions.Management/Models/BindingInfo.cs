using System.Text.Json.Serialization;

namespace RabbitMQ.Abstractions.Management.Models;

public class BindingInfo
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("vhost")]
    public string VirtualHost { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("destination_type")]
    public string DestinationType { get; set; } = string.Empty;

    [JsonPropertyName("routing_key")]
    public string RoutingKey { get; set; } = string.Empty;

    [JsonPropertyName("properties_key")]
    public string PropertiesKey { get; set; } = string.Empty;
}
