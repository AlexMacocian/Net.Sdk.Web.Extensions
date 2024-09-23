using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sybil;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class MapEndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => s is ClassDeclarationSyntax,
            transform: static (ctx, _) => GetFilteredClassDeclarationSyntax(ctx)).Where(static c => c is not null);
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndClasses, (sourceProductionContext, tuple) => Execute(tuple.Left, tuple.Right, sourceProductionContext));
    }

    private static ClassDeclarationSyntax? GetFilteredClassDeclarationSyntax(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (classDeclarationSyntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .OfType<AttributeSyntax>()
            .Any(s => s.Name.ToString() == Constants.GetAttributeName || s.Name.ToString() == Constants.GetAttributeShortName ||
                       s.Name.ToString() == Constants.PostAttributeName || s.Name.ToString() == Constants.PostAttributeShortName ||
                       s.Name.ToString() == Constants.PutAttributeName || s.Name.ToString() == Constants.PutAttributeShortName ||
                       s.Name.ToString() == Constants.DeleteAttributeName || s.Name.ToString() == Constants.DeleteAttributeShortName))
        {
            return classDeclarationSyntax;
        }

        return default;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext sourceProductionContext)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        var maybeLanguageVersion = (compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions)?.LanguageVersion;
        if (!maybeLanguageVersion.HasValue)
        {
            return;
        }

        var languageVersion = maybeLanguageVersion.Value;
        var builder = SyntaxBuilder.CreateCompilationUnit()
            .WithUsing(Constants.UsingSystemThreading)
            .WithUsing(Constants.UsingMicrosoftAspNetCoreRouting)
            .WithUsing(Constants.UsingMicrosoftAspNetCoreHttp)
            .WithUsing(Constants.UsingMicrosoftAspNetCoreBuilder);

        var routeUsings = new HashSet<string>();
        
        var namespaceBuilder = languageVersion >= LanguageVersion.CSharp10 ? SyntaxBuilder.CreateFileScopedNamespace(Constants.Namespace) : SyntaxBuilder.CreateNamespace(Constants.Namespace);
        builder.WithNamespace(namespaceBuilder);

        var extensionClassBuilder = SyntaxBuilder.CreateClass(Constants.EndpointRouteBuilderExtensionsName)
            .WithModifiers($"{Constants.Public} {Constants.Static}");
        namespaceBuilder.WithClass(extensionClassBuilder);

        foreach(var classDeclarationSyntax in classes)
        {
            if (classDeclarationSyntax is null)
            {
                continue;
            }

            if (GetParentOfType<BaseNamespaceDeclarationSyntax>(classDeclarationSyntax) is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
            {
                routeUsings.Add(baseNamespaceDeclarationSyntax.Name.ToString());
            }

            var attributes = classDeclarationSyntax.AttributeLists
                .SelectMany(l => l.Attributes)
                .OfType<AttributeSyntax>().ToList();

            if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.GetAttributeName || a.Name.ToString() == Constants.GetAttributeShortName) is AttributeSyntax getAttributeSyntax)
            {
                var methodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, $"MapGet{classDeclarationSyntax.Identifier}")
                    .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                    .WithModifiers($"{Constants.Public} {Constants.Static}");

                var pattern = getAttributeSyntax.ArgumentList?.Arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == Constants.PatternPropertyName)?.Expression.ToString().Trim('"');
                if (pattern is null)
                {
                    continue;
                }

                methodBuilder.WithBody(@$"
        builder.MapGet(""{pattern}"", async (HttpContext context, {classDeclarationSyntax.Identifier} route) =>
        {{
            var request = await route.PreProcess(context, context.RequestAborted);
            var response = await route.HandleRequest(request, context.RequestAborted);
            return response.GetResult();
        }});

        return builder;");
                extensionClassBuilder.WithMethod(methodBuilder);
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.PostAttributeName || a.Name.ToString() == Constants.PostAttributeShortName) is AttributeSyntax postAttributeSyntax)
            {
                var methodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, $"MapPost{classDeclarationSyntax.Identifier}")
                    .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                    .WithModifiers($"{Constants.Public} {Constants.Static}");

                var pattern = postAttributeSyntax.ArgumentList?.Arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == Constants.PatternPropertyName)?.Expression.ToString().Trim('"');
                if (pattern is null)
                {
                    continue;
                }

                methodBuilder.WithBody(@$"
        builder.MapPost(""{pattern}"", async (HttpContext context, {classDeclarationSyntax.Identifier} route) =>
        {{
            var request = await route.PreProcess(context, context.RequestAborted);
            var response = await route.HandleRequest(request, context.RequestAborted);
            return response.GetResult();
        }});

        return builder;");
                extensionClassBuilder.WithMethod(methodBuilder);
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.PutAttributeName || a.Name.ToString() == Constants.PutAttributeShortName) is AttributeSyntax putAttributeSyntax)
            {
                var methodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, $"MapPut{classDeclarationSyntax.Identifier}")
                    .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                    .WithModifiers($"{Constants.Public} {Constants.Static}");

                var pattern = putAttributeSyntax.ArgumentList?.Arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == Constants.PatternPropertyName)?.Expression.ToString().Trim('"');
                if (pattern is null)
                {
                    continue;
                }

                methodBuilder.WithBody(@$"
        builder.MapPut(""{pattern}"", async (HttpContext context, {classDeclarationSyntax.Identifier} route) =>
        {{
            var request = await route.PreProcess(context, context.RequestAborted);
            var response = await route.HandleRequest(request, context.RequestAborted);
            return response.GetResult();
        }});

        return builder;");
                extensionClassBuilder.WithMethod(methodBuilder);
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.DeleteAttributeName || a.Name.ToString() == Constants.DeleteAttributeShortName) is AttributeSyntax deleteAttributeSyntax)
            {
                var methodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, $"MapDelete{classDeclarationSyntax.Identifier}")
                    .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                    .WithModifiers($"{Constants.Public} {Constants.Static}");

                var pattern = deleteAttributeSyntax.ArgumentList?.Arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == Constants.PatternPropertyName)?.Expression.ToString().Trim('"');
                if (pattern is null)
                {
                    continue;
                }

                methodBuilder.WithBody(@$"
        builder.MapDelete(""{pattern}"", async (HttpContext context, {classDeclarationSyntax.Identifier} route) =>
        {{
            var request = await route.PreProcess(context, context.RequestAborted);
            var response = await route.HandleRequest(request, context.RequestAborted);
            return response.GetResult();
        }});

        return builder;");
                extensionClassBuilder.WithMethod(methodBuilder);
            }
        }

        foreach(var classUsing in routeUsings)
        {
            builder.WithUsing(classUsing);
        }

        var fileSource = builder.Build().ToFullString();
        sourceProductionContext.AddSource($"{Constants.EndpointRouteBuilderExtensionsName}.g", fileSource);
    }

    private static T? GetParentOfType<T>(SyntaxNode syntaxNode)
    {
        if (syntaxNode.Parent is null)
        {
            return default;
        }

        if (syntaxNode.Parent is T parentNode)
        {
            return parentNode;
        }

        return GetParentOfType<T>(syntaxNode.Parent);
    }
}

#nullable disable