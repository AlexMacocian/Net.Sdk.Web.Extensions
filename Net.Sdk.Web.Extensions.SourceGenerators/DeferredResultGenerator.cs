using Microsoft.CodeAnalysis;
using Sybil;
using System;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class DeferredResultGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateRouteFilterAttribute);
    }

    private static void GenerateRouteFilterAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        var builder = SyntaxBuilder.CreateCompilationUnit()
                .WithNamespace(
                SyntaxBuilder.CreateNamespace(Constants.Namespace)
                    .WithUsing(Constants.UsingSystemThreading)
                    .WithUsing(Constants.UsingMicrosoftAspNetCoreHttp)
                    .WithClass(SyntaxBuilder.CreateClass(Constants.DeferredResultClass)
                        .WithBaseClass(Constants.IResultTypeName)
                        .WithModifier(Constants.Public)
                        .WithModifier(Constants.Sealed)
                        .WithField(SyntaxBuilder.CreateField($"Task<{Constants.IResultInterface}>", "inner")
                            .WithModifier(Constants.Public)
                            .WithModifier(Constants.Readonly))
                        .WithConstructor(SyntaxBuilder.CreateConstructor(Constants.DeferredResultClass)
                            .WithModifier(Constants.Public)
                            .WithParameter($"Task<{Constants.IResultInterface}>", "inner")
                            .WithBody("this.inner = inner;"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.Task, Constants.ExecuteAsyncMethodName)
                            .WithParameter("HttpContext", "httpContext")
                            .WithModifier(Constants.Public)
                            .WithModifier(Constants.Async)
                            .WithBody(@$"
        var result = await this.inner;
        await result.ExecuteAsync(httpContext);"))));

        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{Constants.DeferredResultClass}.g", $"{source}");
    }
}

#nullable disable