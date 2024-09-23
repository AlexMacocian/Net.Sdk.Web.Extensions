using Microsoft.CodeAnalysis;
using Sybil;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class RouteInterfaceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            GenerateInterface(context);
        });
    }

    private static void GenerateInterface(IncrementalGeneratorPostInitializationContext context)
    {
        var builder = SyntaxBuilder.CreateCompilationUnit()
            .WithNamespace(SyntaxBuilder.CreateNamespace(Constants.Namespace)
                .WithInterface(SyntaxBuilder.CreateInterface(Constants.RouteInterface)
                    .WithTypeParameter(SyntaxBuilder.CreateTypeParameter(Constants.RouteTypeParameter))
                    .WithTypeParameterConstraint(SyntaxBuilder.CreateTypeParameterConstraint(Constants.RouteTypeParameter)
                        .WithClass())
                    .WithMethod(SyntaxBuilder.CreateMethod(Constants.PreProcessRequestReturnType, Constants.PreProcessRequestMethodName)
                        .WithModifier(Constants.Public)
                        .WithParameter(Constants.HttpContextTypeName, Constants.HttpContextParameterName)
                        .WithParameter(Constants.CancellationTokenTypeName, Constants.CancellationTokenParameterName)
                        .WithNoBody())
                    .WithMethod(SyntaxBuilder.CreateMethod(Constants.HandleRequestReturnType, Constants.HandleRequestMethodName)
                        .WithModifier(Constants.Public)
                        .WithParameter($"{Constants.RouteTypeParameter}?", Constants.RequestParameterName)
                        .WithParameter(Constants.CancellationTokenTypeName, Constants.CancellationTokenParameterName)
                        .WithNoBody())));

        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{Constants.RouteFileName}.g", source);
    }
}

#nullable disable