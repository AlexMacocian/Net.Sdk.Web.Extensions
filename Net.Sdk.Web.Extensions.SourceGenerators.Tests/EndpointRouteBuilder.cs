
namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests;

public class EndpointRouteBuilder : IEndpointRouteBuilder
{
    public IServiceProvider ServiceProvider { get; }
    public ICollection<EndpointDataSource> DataSources { get; }

    public IApplicationBuilder CreateApplicationBuilder()
    {
        throw new NotImplementedException();
    }
}
