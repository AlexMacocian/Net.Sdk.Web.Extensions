using AspNetCore.Extensions;
using AspNetCore.Extensions.Options;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Options;
using System.Core.Extensions;

namespace Net.Sdk.Web.Extensions.Http;

public sealed class CorrelationVectorHandler : DelegatingHandler
{
    private readonly CorrelationVectorOptions options;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CorrelationVectorHandler(
        IOptions<CorrelationVectorOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        this.options = options.ThrowIfNull().Value.ThrowIfNull();
        this.httpContextAccessor = httpContextAccessor.ThrowIfNull();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (this.httpContextAccessor.HttpContext?.GetCorrelationVector() is CorrelationVector cv)
        {
            cv.Increment();
            request.Headers.Add(this.options.Header, cv.Value);
            this.httpContextAccessor.HttpContext.SetCorrelationVector(cv);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.Headers.TryGetValues(this.options.Header, out var cvHeaders) &&
            cvHeaders.FirstOrDefault() is string responseCv)
        {
            this.httpContextAccessor.HttpContext?.SetCorrelationVector(CorrelationVector.Parse(responseCv));
        }

        return response;
    }
}
