using Microsoft.CodeAnalysis;
using Sybil;
using System;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class ControllerAttributeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateAttribute);
    }

    private static void GenerateAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        var builder = SyntaxBuilder.CreateCompilationUnit()
            .WithNamespace(SyntaxBuilder.CreateNamespace(Constants.Namespace)
                .WithClass(SyntaxBuilder.CreateClass(Constants.RouteAttributeName)
                    .WithBaseClass(nameof(Attribute))
                    .WithModifiers($"{Constants.Public} {Constants.Sealed}")
                    .WithAttribute(SyntaxBuilder.CreateAttribute("AttributeUsage")
                        .WithArgument(AttributeTargets.Class)
                        .WithArgument("Inherited", false)
                        .WithArgument("AllowMultiple", false))
                    .WithProperty(SyntaxBuilder.CreateProperty(Constants.StringType, Constants.PatternPropertyName)
                        .WithModifier(Constants.Public)
                        .WithAccessor(SyntaxBuilder.CreateGetter())
                        .WithAccessor(SyntaxBuilder.CreateSetter()))));

        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{Constants.RouteAttributeName}.g", source);
    }
}

#nullable disable