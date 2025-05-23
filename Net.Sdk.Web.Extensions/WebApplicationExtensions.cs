using Net.Sdk.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Core.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Extensions;
using System.Net.WebSockets;
using Microsoft.IO;

namespace Net.Sdk.Web.Websockets.Extensions;

public static class WebApplicationExtensions
{
    private static readonly RecyclableMemoryStreamManager StreamManager = new();

    public static WebApplication UseCorrelationVector(this WebApplication webApplication)
    {
        webApplication.ThrowIfNull()
            .UseMiddleware<CorrelationVectorMiddleware>();

        return webApplication;
    }

    public static WebApplication UseIPExtraction(this WebApplication webApplication)
    {
        webApplication.ThrowIfNull()
            .UseMiddleware<IPExtractingMiddleware>();

        return webApplication;
    }

    public static IEndpointConventionBuilder UseWebSocketRoute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(this WebApplication app, string route)
        where TWebSocketRoute : WebSocketRouteBase
    {
        app.ThrowIfNull();
        return app.MapGet(route, async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var logger = app.Services.GetRequiredService<ILogger<TWebSocketRoute>>();
                var route = app.Services.GetRequiredService<TWebSocketRoute>();
                var routeFilters = GetRouteFilters<TWebSocketRoute>(context).ToList();

                var actionContext = new ActionContext(
                        context,
                        new RouteData(),
                        new ActionDescriptor());
                var actionExecutingContext = new ActionExecutingContext(
                        actionContext,
                        routeFilters,
                        new Dictionary<string, object?>(),
                        route);
                var actionExecutedContext = new ActionExecutedContext(
                        actionContext,
                        routeFilters,
                        route);
                try
                {
                    var processingTask = new Func<Task>(() => ProcessWebSocketRequest(route, context));
                    await BeginProcessingPipeline(actionExecutingContext, actionExecutedContext, processingTask);
                }
                catch(WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    logger.LogInformation("Websocket closed prematurely. Marking as closed");
                }
                catch(OperationCanceledException)
                {
                    logger.LogInformation("Websocket closed prematurely. Marking as closed");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Encountered exception while handling websocket. Closing");
                }
                finally
                {
                    await route.SocketClosed();
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });
    }

    private static async Task ProcessWebSocketRequest(WebSocketRouteBase route, HttpContext httpContext)
    {
        using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        route.WebSocket = webSocket;
        route.Context = httpContext;
        await route.SocketAccepted(httpContext.RequestAborted);
        httpContext.Request.Scheme = httpContext.Request.Scheme == "https" ? "wss" : "ws";
        await HandleWebSocket(webSocket, route, httpContext.RequestAborted);
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", httpContext.RequestAborted);
    }

    private static async Task BeginProcessingPipeline(ActionExecutingContext actionExecutingContext, ActionExecutedContext actionExecutedContext, Func<Task> processWebSocket)
    {
        foreach (var filter in actionExecutingContext.Filters.OfType<IActionFilter>())
        {
            filter.OnActionExecuting(actionExecutingContext);
            if (actionExecutingContext.Result is IActionResult result)
            {
                await result.ExecuteResultAsync(actionExecutedContext);
                return;
            }
        }

        ActionExecutionDelegate pipeline = async () =>
        {
            await processWebSocket();
            return actionExecutedContext;
        };

        foreach (var filter in actionExecutingContext.Filters.OfType<IAsyncActionFilter>())
        {
            var next = pipeline;
            pipeline = async () =>
            {
                if (actionExecutingContext.Result is IActionResult result)
                {
                    await result.ExecuteResultAsync(actionExecutedContext);
                    return actionExecutedContext;
                }

                await filter.OnActionExecutionAsync(actionExecutingContext, next);
                await (actionExecutingContext.Result?.ExecuteResultAsync(actionExecutingContext) ?? Task.CompletedTask);
                return actionExecutedContext;
            };
        }

        await pipeline();
    }

    private static async Task HandleWebSocket(WebSocket webSocket, WebSocketRouteBase route, CancellationToken cancellationToken)
    {
        if (route.Context is null)
        {
            throw new InvalidOperationException("Route HttpContext is null");
        }

        using var ms = StreamManager.GetStream();
        ValueWebSocketReceiveResult result;
        while(webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            do
            {
                var buffer = ms.GetMemory(8 * 1024);
                result = await webSocket.ReceiveAsync(buffer, cancellationToken);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return;
            }

            await route.ExecuteAsync(result.MessageType, ms.GetReadOnlySequence(), cancellationToken);
        }
    }

    private static IEnumerable<IFilterMetadata> GetRouteFilters<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(HttpContext context)
        where TWebSocketRoute : WebSocketRouteBase
    {
        foreach(var attribute in typeof(TWebSocketRoute).GetCustomAttributes(true).OfType<ServiceFilterAttribute>())
        {
            var filter = context.RequestServices.GetRequiredService(attribute.ServiceType);
            yield return filter.Cast<IFilterMetadata>();
        }
    }
}
