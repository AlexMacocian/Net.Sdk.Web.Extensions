using Microsoft.CorrelationVector;
using System.Core.Extensions;

namespace Net.Sdk.Web;

public static class HttpContextExtensions
{
    private const string CorrelationVectorKey = "CorrelationVector";
    private const string ClientIPKey = "ClientIP";

    public static void SetCorrelationVector(this HttpContext context, CorrelationVector cv)
    {
        context.ThrowIfNull()
            .Items.Add(CorrelationVectorKey, cv);
    }

    public static CorrelationVector GetCorrelationVector(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(CorrelationVectorKey, out var cvVal) ||
            cvVal is not CorrelationVector cv)
        {
            throw new InvalidOperationException("Unable to extract API Key from context");
        }

        return cv;
    }

    public static void SetClientIP(this HttpContext context, string ip)
    {
        context.ThrowIfNull()
            .Items.Add(ClientIPKey, ip);
    }

    public static string GetClientIP(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(ClientIPKey, out var ip) ||
            ip is not string ipStr)
        {
            throw new InvalidOperationException("Unable to extract IP from context");
        }

        return ipStr;
    }
}
