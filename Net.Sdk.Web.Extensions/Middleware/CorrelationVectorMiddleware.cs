using Net.Sdk.Web.Options;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Options;
using System.Core.Extensions;

namespace Net.Sdk.Web.Middleware;

public sealed class CorrelationVectorMiddleware : IMiddleware
{
    private readonly CorrelationVectorOptions options;

    public CorrelationVectorMiddleware() : this(Microsoft.Extensions.Options.Options.Create<CorrelationVectorOptions>(new()))
    {
    }

    public CorrelationVectorMiddleware(IOptions<CorrelationVectorOptions> options)
    {
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cv = new CorrelationVector();
        if (context.Items.TryGetValue(this.options.Header, out var correlationVectorVal) &&
            correlationVectorVal is string correlationVectorStr)
        {
            cv = CorrelationVector.Parse(correlationVectorStr);
            cv.Increment();
        }

        context.SetCorrelationVector(cv);
        return next(context);
    }
}
