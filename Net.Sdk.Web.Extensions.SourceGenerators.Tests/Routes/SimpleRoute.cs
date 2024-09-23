using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Filters;
using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Models;

namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateMapGet(Pattern = "simple")]
[RouteFilter(RouteFilterType = typeof(SimpleFilter))]
public sealed class SimpleRoute : IRoute<SimpleRequest>
{
    public Task<Response> HandleRequest(SimpleRequest? request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<SimpleRequest?> PreProcess(HttpContext? context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
