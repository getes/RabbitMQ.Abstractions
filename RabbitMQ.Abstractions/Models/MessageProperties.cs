namespace RabbitMQ.Abstractions.Models;

public class MessageProperties
{
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ContentType { get; set; }
    public string? ContentEncoding { get; set; }
    public byte? DeliveryMode { get; set; } = 2; // Persistent
    public byte? Priority { get; set; }
    public string? ReplyTo { get; set; }
    public string? Expiration { get; set; }
    public string? Type { get; set; }
    public string? AppId { get; set; }
    public IDictionary<string, object?> Headers { get; set; } = new Dictionary<string, object?>();
}
