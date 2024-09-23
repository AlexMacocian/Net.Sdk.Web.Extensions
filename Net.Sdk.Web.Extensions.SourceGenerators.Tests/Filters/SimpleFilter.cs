
namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Filters;

public class SimpleFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        throw new NotImplementedException();
    }
}
