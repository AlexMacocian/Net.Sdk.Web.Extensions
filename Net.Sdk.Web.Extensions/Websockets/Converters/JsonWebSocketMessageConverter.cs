using Net.Sdk.Web.Attributes;
using System.Buffers;
using System.Core.Extensions;
using System.Text;
using System.Text.Json;

namespace Net.Sdk.Web.Websockets.Converters;

public class JsonWebSocketMessageConverter<T> : WebSocketMessageConverter<T>
{
    private readonly JsonSerializerOptions? jsonSerializerOptions;

    [DoNotInject]
    public JsonWebSocketMessageConverter()
    {
    }

    public JsonWebSocketMessageConverter(JsonSerializerOptions options)
    {
        this.jsonSerializerOptions = options.ThrowIfNull();
    }

    public override T ConvertTo(WebSocketConverterRequest request)
    {
        if (request.Type != System.Net.WebSockets.WebSocketMessageType.Text)
        {
            throw new InvalidOperationException($"Unable to deserialize message. Message is not text");
        }

        if (request.Payload is null)
        {
            throw new InvalidOperationException($"Unable to deserialize message. Payload is null");
        }

        var stringData = Encoding.UTF8.GetString(request.Payload.Value);
        var objData = JsonSerializer.Deserialize<T>(stringData, this.jsonSerializerOptions);
        return objData ?? throw new InvalidOperationException($"Unable to deserialize message to {typeof(T).Name}");
    }

    public override WebSocketConverterResponse ConvertFrom(T message)
    {
        var serialized = JsonSerializer.Serialize(message, this.jsonSerializerOptions);
        var data = Encoding.UTF8.GetBytes(serialized);
        var readOnlySequence = new ReadOnlySequence<byte>(data);
        return new WebSocketConverterResponse { EndOfMessage = true, Type = System.Net.WebSockets.WebSocketMessageType.Text, Payload = readOnlySequence };
    }
}
