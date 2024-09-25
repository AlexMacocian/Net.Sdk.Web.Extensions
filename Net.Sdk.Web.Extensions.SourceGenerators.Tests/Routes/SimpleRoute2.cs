namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateRoute]
public class SimpleRoute2
{
    [GenerateMapPost(Pattern = "somethingSimple")]
    public async Task<IResult> GetSomething()
    {

    }
}
