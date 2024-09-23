using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Models;

namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateMapDelete(Pattern = "simple")]
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
