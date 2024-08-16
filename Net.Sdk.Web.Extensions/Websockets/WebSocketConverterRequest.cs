using System.Net.WebSockets;

namespace AspNetCore.Extensions.Websockets;

public sealed class WebSocketConverterRequest
{
    public WebSocketMessageType Type { get; set; }
    public byte[]? Payload { get; set; }
}

