using Microsoft.AspNetCore.Mvc;
using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Filters;
using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Models;

namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateRoute(Pattern = "simple")]
[RouteFilter(RouteFilterType = typeof(SimpleFilter))]
[RouteFilter(RouteFilterType = typeof(SimpleFilter))]
public sealed class SimpleRoute
{
    [GenerateMapGet(Pattern = "get/{api}/{id}")]
    public async Task<IResult> GetSimple(string api, string id, [FromBody] SimpleRequest reques)
    {
        return Results.Ok();
    }
}
