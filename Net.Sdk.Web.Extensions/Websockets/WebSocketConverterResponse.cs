using System.Net.WebSockets;

namespace Net.Sdk.Web.Websockets;

public sealed class WebSocketConverterResponse
{
    public WebSocketMessageType Type { get; set; }
    public byte[]? Payload { get; set; }
    public bool EndOfMessage { get; set; }
}