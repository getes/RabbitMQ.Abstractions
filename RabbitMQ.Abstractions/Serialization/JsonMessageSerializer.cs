using System.Text.Json;

namespace RabbitMQ.Abstractions.Serialization;

public sealed class JsonMessageSerializer : IMessageSerializer
{
    private readonly JsonSerializerOptions _options;

    public string ContentType => "application/json";

    public JsonMessageSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    public JsonMessageSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public byte[] Serialize<T>(T message) where T : class
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, _options);
    }

    public T? Deserialize<T>(ReadOnlySpan<byte> data) where T : class
    {
        return JsonSerializer.Deserialize<T>(data, _options);
    }
}
