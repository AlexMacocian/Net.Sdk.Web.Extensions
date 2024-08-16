# Net.Sdk.Web.Extensions

.NET library extending functionality for web applications

# Features
- [Improved Websocket Support](#better-websocket-support)
- [Improved request tracing](#request-tracing-using-correlationvectors)
- [Improved client IP handling](#client-ip-extraction)
- [Improved configuration binding](#better-configuration-binding-support)
- [Mockable IHttpClient builder](#mockable-httpclient)
- [Support for base64 encoded certificates in configuration](#base64-to-x509certificate2-converter-for-options)

## Better WebSocket support

Net.Sdk.Web.Extensions provides a streamlined implementation of WebSockets to be used in a web app, in a syntax similar to Asp Mvc

### Integration into `WebApplication` 
```C#
var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();
app.MapWebSocket<CustomRoute>("custom-route");
```

### Message mapping
#### Define your request
```C#
[WebSocketConverter<JsonWebSocketMessageConverter<CustomRequest>, CustomRequest>]
public class CustomRequest{
}
```

#### Define your response
```C#
[WebSocketConverter<JsonWebSocketMessageConverter<CustomResponse>, CustomResponse>]
public class CustomResponse{
}
```

#### Implement your WebSocketRoute
```C#
public class CustomRoute : WebSocketRouteBase<CustomRequest, CustomResponse>
{
    public override Task SocketAccepted(CancellationToken cancellationToken)
    {
        return base.SocketAccepted(cancellationToken);
    }

    public override Task SocketClosed()
    {
        return base.SocketClosed();
    }

    public override async Task ExecuteAsync(CustomRequest? request, CancellationToken cancellationToken)
    {
        await this.SendMessage(new BotResponse(), cancellationToken);
    }
}
```

### Custom message converters
If you need specialized converters, implement your own `WebSocketMessageConverter<T>`

## Request tracing using CorrelationVectors

### Integration into `WebApplication`
```C#
var builder = WebApplication.CreateSlimBuilder(args);
builder.WithCorrelationVector();
var app = builder.Build();
app.UseCorrelationVector();
```

### Accessing the correlation vector
Take a dependency on `IHttpContextAccessor` and get the CV using the `HttpContextExtensions.GetCorrelationVector` method
```C#
var cv = accessor.HttpContext.GetCorrelationVector();
```

### Configuration
Configure an instance of `CorrelationVectorOptions` to adjust the CV header

### Requests and responses
- On a request, the `CorrelationVectorMiddleware` retrieves the CV from the request header if present, otherwise it creates a new one
- The CV is stored in `HttpContext.Items` under `CorrelationVector` key
- Log the CV to follow traces of one application flow under multiple operations and across http requests

### Integration with `HttpClient`
- Pass the `CorrelationVectorHandler` to the `IHttpClientBuilder` to manage CVs across requests
- On each `HttpClient` request, the CV is added to the configured header
- After receiving the response, the `CorrelationVectorHandler` will receive and parse any existing CV from the headers and reapply it to the `HttpContext.Items`

## Client IP extraction
`IPExtractingMiddleware` figures out the IP of the client, being able to handle reverse proxying through `X-Forwarded-For` header as well as CloudFlare specific `CF-Connecting-IP` header

### Integration into `WebApplication`
```C#
var builder = WebApplication.CreateSlimBuilder(args);
builder.WithIPExtraction();
var app = builder.Build();
app.UseIPExtraction();
```

### Accessing the client IP
Take a dependency on `IHttpContextAccessor` and get the CV using the `HttpContextExtensions.GetCorrelationVector` method
```C#
var cv = accessor.HttpContext.GetClientIP();
```

## Better configuration binding support

### Integration into `WebApplication`
```C#
var builder = WebApplication.CreateSlimBuilder(args);
builder.ConfigureExtended<CustomOptions>();

public class CustomService 
{
    public CustomService (IOptions<CustomOptions> options)
    {
    }
}
```

### Customizing configuration key
```C#
[OptionsName(Name = "CustomKey")]
public class CustomOptions
{

}
```

## Mockable HttpClient
### Integration into `WebApplication`
```C#
var builder = WebApplication.CreateSlimBuilder(args);
builder.RegisterHttpClient<CustomService>()
    .WithTimeout(TimeSpan.FromSeconds(5))
    .WithCorrelationVector()
    .CreateBuilder();

public class CustomService
{
    public CustomService (IHttpClient<CustomService> client)
    {
    }
}
```

## Base64 to X509Certificate2 converter for options
Use the `Base64ToCertificateConverter` to retrieve your SSL certificate from configuration and bind it to options, to allow for dynamic loading of certificates

### Use `Base64ToCertificateConverter` on your `X509Certificate2` property
```C#
public class ServerOptions
{
    [JsonConverter(typeof(Base64ToCertificateConverter))]
    public X509Certificate2 Certificate { get; set; } = default!;
}
```