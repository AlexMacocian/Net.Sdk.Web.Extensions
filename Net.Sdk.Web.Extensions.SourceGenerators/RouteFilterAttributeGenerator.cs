using Microsoft.CodeAnalysis;
using Sybil;
using System;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class RouteFilterAttributeGenerator : IIncrementalGenerator
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
                    .WithClass(SyntaxBuilder.CreateClass(Constants.RouteFilterAttributeName)
                        .WithModifier(Constants.Public)
                        .WithTypeParameter(SyntaxBuilder.CreateTypeParameter(Constants.T))
                        .WithTypeParameterConstraint(SyntaxBuilder.CreateTypeParameterConstraint(Constants.T)
                            .WithType(Constants.IEndpointFilterName))
                        .WithConstructor(SyntaxBuilder.CreateConstructor(Constants.RouteFilterAttributeName)
                            .WithModifier(Constants.Public)
                            .WithBody($"this.{Constants.RouteFilterTypePropertyName} = typeof({Constants.T});"))
                        .WithAttribute(SyntaxBuilder.CreateAttribute("AttributeUsage")
                            .WithArgument(AttributeTargets.All)
                            .WithArgument("Inherited", false)
                            .WithArgument("AllowMultiple", true))
                        .WithBaseClass(nameof(Attribute))
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.TypeType}?", Constants.RouteFilterTypePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter()))));


        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{Constants.RouteFilterAttributeName}.g", $"#nullable enable\n{source}\n#nullable disable\n");
    }
}

#nullable disable