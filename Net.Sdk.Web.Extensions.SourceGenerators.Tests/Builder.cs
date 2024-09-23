namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests;

public static class Builder
{
    public static void Build()
    {
        EndpointRouteBuilder builder = new EndpointRouteBuilder();
        builder.MapGetSimpleRoute();
    }
}
