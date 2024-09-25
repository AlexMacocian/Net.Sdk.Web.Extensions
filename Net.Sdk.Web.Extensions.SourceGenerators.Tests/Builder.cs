namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests;

public static class Builder
{
    public static void Build()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WithRoutes();

        var app = builder.Build();

        app.UseRoutes();
    }
}
