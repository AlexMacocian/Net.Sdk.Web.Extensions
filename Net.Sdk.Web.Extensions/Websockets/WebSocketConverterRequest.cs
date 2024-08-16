using System.Net.WebSockets;

namespace Net.Sdk.Web.Websockets;

public sealed class WebSocketConverterRequest
{
    public WebSocketMessageType Type { get; set; }
    public byte[]? Payload { get; set; }
}

