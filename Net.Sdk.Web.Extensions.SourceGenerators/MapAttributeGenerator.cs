using System;
using Microsoft.CodeAnalysis;
using Sybil;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class MapAttributeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            GenerateMapAttribute(context, Constants.GetAttributeName);
            GenerateMapAttribute(context, Constants.PostAttributeName);
            GenerateMapAttribute(context, Constants.PutAttributeName);
            GenerateMapAttribute(context, Constants.DeleteAttributeName);
        });
    }

    private static void GenerateMapAttribute(IncrementalGeneratorPostInitializationContext context, string attributeName)
    {
        var builder = SyntaxBuilder.CreateCompilationUnit()
                .WithNamespace(
                SyntaxBuilder.CreateNamespace(Constants.Namespace)
                    .WithClass(SyntaxBuilder.CreateClass(attributeName)
                        .WithModifier(Constants.Public)
                        .WithConstructor(SyntaxBuilder.CreateConstructor(attributeName)
                            .WithModifier(Constants.Public))
                        .WithAttribute(SyntaxBuilder.CreateAttribute("AttributeUsage")
                            .WithArgument(AttributeTargets.Method)
                            .WithArgument("Inherited", false)
                            .WithArgument("AllowMultiple", false))
                        .WithBaseClass(nameof(Attribute))
                        .WithProperty(SyntaxBuilder.CreateProperty(Constants.StringType, Constants.PatternPropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter()))));

        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{attributeName}.g", source);
    }
}

#nullable disable