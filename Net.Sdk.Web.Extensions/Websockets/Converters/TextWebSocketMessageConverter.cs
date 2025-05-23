using System.Buffers;
using System.Text;

namespace Net.Sdk.Web.Websockets.Converters;

public sealed class TextWebSocketMessageConverter : WebSocketMessageConverter<TextContent>
{
    public override TextContent ConvertTo(WebSocketConverterRequest request)
    {
        if (request.Type is not System.Net.WebSockets.WebSocketMessageType.Text)
        {
            throw new InvalidOperationException($"Cannot parse message of type {request.Type}");
        }

        if (request.Payload is null)
        {
            throw new InvalidOperationException($"Unable to serialize message. Payload is null");
        }

        var message = Encoding.UTF8.GetString(request.Payload.Value);
        return new TextContent { Text = message };
    }

    public override WebSocketConverterResponse ConvertFrom(TextContent message)
    {
        return new WebSocketConverterResponse
        {
            Type = System.Net.WebSockets.WebSocketMessageType.Text,
            EndOfMessage = true,
            Payload = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(message.Text ?? string.Empty))
        };
    }
}
