using Net.Sdk.Web.Attributes;
using Net.Sdk.Web.Middleware;
using Net.Sdk.Web.Extensions.Http;
using System.Core.Extensions;
using System.Extensions;
using Net.Sdk.Web.Websockets;
using System.Diagnostics.CodeAnalysis;

namespace Net.Sdk.Web;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder WithWebSocketRoute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWebSocketRoute>(this WebApplicationBuilder builder)
        where TWebSocketRoute : WebSocketRouteBase
    {
        builder.Services.AddScoped<TWebSocketRoute>();
        return builder;
    }

    public static WebApplicationBuilder ConfigureExtended<TOptions>(this WebApplicationBuilder builder)
        where TOptions : class, new()
    {
        builder.ThrowIfNull()
               .Services.Configure<TOptions>(builder.Configuration.GetSection(GetOptionsName<TOptions>()));
        return builder;
    }

    public static IConfigurationSection GetRequiredSection<TOptions>(this ConfigurationManager configurationManager)
    {
        configurationManager.ThrowIfNull();
        return configurationManager.GetRequiredSection(GetOptionsName<TOptions>());
    }

    public static WebApplicationBuilder WithCorrelationVector(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull();
        builder.Services.AddScoped<CorrelationVectorMiddleware>();
        builder.Services.AddScoped<CorrelationVectorHandler>();

        return builder;
    }

    public static WebApplicationBuilder WithIPExtraction(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull();
        builder.Services.AddScoped<IPExtractingMiddleware>();

        return builder;
    }

    public static Extensions.Http.HttpClientBuilder<T> RegisterHttpClient<T>(this WebApplicationBuilder builder)
        where T : class
    {
        _ = builder.ThrowIfNull();
        return new Extensions.Http.HttpClientBuilder<T>(builder);
    }

    public static Extensions.Http.HttpClientBuilder<T> WithCorrelationVector<T>(this Extensions.Http.HttpClientBuilder<T> httpClientBuilder)
        where T : class
    {
        return httpClientBuilder.ThrowIfNull()
            .WithDelegatingHandler(sp => sp.GetRequiredService<CorrelationVectorHandler>());
    }

    public static IHttpClientBuilder WithCorrelationVector<T>(this IHttpClientBuilder httpClientBuilder)
        where T : class
    {
        return httpClientBuilder.ThrowIfNull()
            .AddHttpMessageHandler<CorrelationVectorHandler>();
    }

    private static string GetOptionsName<TOptions>()
    {
        var maybeAttribute = typeof(TOptions).GetCustomAttributes(false).OfType<OptionsNameAttribute>().FirstOrDefault();
        if (maybeAttribute is not null &&
            maybeAttribute.Name?.IsNullOrWhiteSpace() is false)
        {
            return maybeAttribute.Name;
        }

        return typeof(TOptions).Name;
    }
}
