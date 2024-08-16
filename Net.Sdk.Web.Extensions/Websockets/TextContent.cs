using Net.Sdk.Web.Websockets.Converters;

namespace Net.Sdk.Web.Websockets;

[WebSocketConverter<TextWebSocketMessageConverter, TextContent>]
public sealed class TextContent
{
    public string? Text { get; set; }
}

