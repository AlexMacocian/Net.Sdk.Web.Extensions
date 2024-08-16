using AspNetCore.Extensions.Websockets.Converters;

namespace AspNetCore.Extensions.Websockets;

[WebSocketConverter<TextWebSocketMessageConverter, TextContent>]
public sealed class TextContent
{
    public string? Text { get; set; }
}

