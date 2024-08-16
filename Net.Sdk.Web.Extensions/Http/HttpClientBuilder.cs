using System.Core.Extensions;

namespace Net.Sdk.Web.Extensions.Http;

public sealed class HttpClientBuilder<T>
    where T : class
{
    private readonly WebApplicationBuilder app;
    private readonly List<Func<IServiceProvider, DelegatingHandler>> handlers = [];
    private readonly List<Func<IServiceProvider, (string Name, IEnumerable<string> Values)>> defaultRequestHeaders = [];
    private readonly List<string> redactedLoggedHeaderNames = [];
    private readonly List<Func<string, bool>> headerRedactHandlers = [];

    private bool defaultLogger = false;
    private Uri? baseAddress;
    private long maxResponseBufferSize = 2147483647L; //2GB default HttpClient value [https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.maxresponsecontentbuffersize?view=net-6.0]
    private TimeSpan timeout = TimeSpan.FromSeconds(100); //100 seconds default HttpClient value [https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout?view=net-6.0]

    internal HttpClientBuilder(WebApplicationBuilder app)
    {
        this.app = app.ThrowIfNull();
    }

    public HttpClientBuilder<T> WithBaseAddress(Uri baseAddress)
    {
        baseAddress.ThrowIfNull();

        this.baseAddress = baseAddress;
        return this;
    }

    public HttpClientBuilder<T> WithDelegatingHandler(Func<IServiceProvider, DelegatingHandler> handlerFactory)
    {
        handlerFactory.ThrowIfNull();

        this.handlers.Add(handlerFactory);
        return this;
    }

    public HttpClientBuilder<T> WithDefaultRequestHeaders(Func<IServiceProvider, (string Name, IEnumerable<string> Values)> headerFactory)
    {
        headerFactory.ThrowIfNull();

        this.defaultRequestHeaders.Add(headerFactory);
        return this;
    }

    public HttpClientBuilder<T> WithMaxResponseBufferSize(long responseBufferSize)
    {
        this.maxResponseBufferSize = responseBufferSize;
        return this;
    }

    public HttpClientBuilder<T> WithRedactedLoggerHeaderName(string headerName)
    {
        this.redactedLoggedHeaderNames.Add(headerName);
        return this;
    }

    public HttpClientBuilder<T> WithRedactedLoggerHeaderName(Func<string, bool> handler)
    {
        this.headerRedactHandlers.Add(handler);
        return this;
    }

    public HttpClientBuilder<T> WithTimeout(TimeSpan timeout)
    {
        this.timeout = timeout;
        return this;
    }

    public HttpClientBuilder<T> AddDefaultLogger()
    {
        this.defaultLogger = true;
        return this;
    }

    public WebApplicationBuilder Build()
    {
        this.CreateBuilder();
        return this.app;
    }

    public IHttpClientBuilder CreateBuilder()
    {
        var builder = this.app.Services.AddHttpClient(typeof(T).Name);
        foreach (var handler in this.handlers)
        {
            builder.AddHttpMessageHandler(handler);
        }

        builder.ConfigureHttpClient((sp, client) =>
        {
            client.BaseAddress = this.baseAddress;
            client.Timeout = this.timeout;
            client.MaxResponseContentBufferSize = this.maxResponseBufferSize;
            foreach (var header in this.defaultRequestHeaders)
            {
                (var name, var values) = header(sp);
                client.DefaultRequestHeaders.Add(name, values);
            }
        });

        if (this.defaultLogger)
        {
            builder.AddDefaultLogger();
        }

        if (this.redactedLoggedHeaderNames.Count > 0)
        {
            builder.RedactLoggedHeaders(this.redactedLoggedHeaderNames);
        }

        foreach (var handler in this.headerRedactHandlers)
        {
            builder.RedactLoggedHeaders(handler);
        }

        this.app.Services.AddScoped<IHttpClient<T>>(sp =>
        {
            var clientBuilder = sp.GetRequiredService<IHttpClientFactory>();
            var innerClient = clientBuilder.CreateClient(typeof(T).Name);
            return new HttpClient<T>(innerClient);
        });

        return builder;
    }
}