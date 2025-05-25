using System.Core.Extensions;

namespace Net.Sdk.Web.Websockets;

public sealed class DeferredWebSocketResult(
    WebSocketRouteBase route,
    Task inner) : IResult
{
    private readonly WebSocketRouteBase route = route.ThrowIfNull();
    private readonly Task inner = inner.ThrowIfNull();

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        try
        {
            await this.inner;
        }
        finally
        {
            await this.route.SocketClosed();
        }
    }
}
