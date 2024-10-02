using Microsoft.AspNetCore.Mvc;
using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Filters;
using Net.Sdk.Web.Extensions.SourceGenerators.Tests.Models;

namespace Net.Sdk.Web.Extensions.SourceGenerators.Tests.Routes;

[GenerateController("simple")]
[RouteFilter<SimpleFilter>]
public sealed class SimpleRoute
{
    [GenerateGet("get/{api}/{id}")]
    public async Task<IResult> GetSimple(string api, string id, [FromBody] SimpleRequest reques)
    {
        return Results.Ok();
    }
}
