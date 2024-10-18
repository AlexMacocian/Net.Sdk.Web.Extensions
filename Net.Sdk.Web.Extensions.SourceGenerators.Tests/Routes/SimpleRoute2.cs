namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateController]
public class SimpleRoute2
{
    [GeneratePost("somethingSimple")]
    public IResult GetSomething()
    {
        return Results.Ok();
    }
}
