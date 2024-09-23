using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sybil;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class MapEndpointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateRouteFilterAttribute);

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

        var webAppBuilder = SyntaxBuilder.CreateClass(Constants.WebApplicationExtensionsName)
            .WithModifiers($"{Constants.Public} {Constants.Static}");

        var extensionClassBuilder = SyntaxBuilder.CreateClass(Constants.EndpointRouteBuilderExtensionsName)
            .WithModifiers($"{Constants.Public} {Constants.Static}");
        namespaceBuilder.WithClass(webAppBuilder);
        namespaceBuilder.WithClass(extensionClassBuilder);

        var useRoutesWebAppMethodBuilder = SyntaxBuilder.CreateMethod(Constants.WebApplicationTypeName, Constants.UseRoutesMethodName)
                .WithThisParameter(Constants.WebApplicationTypeName, Constants.BuilderParameterName)
                .WithModifiers($"{Constants.Public} {Constants.Static}");

        var useRoutesMethodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, Constants.MapAllRoutes)
                .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                .WithModifiers($"{Constants.Public} {Constants.Static}");
        var useRoutesBody = new StringBuilder();
        var useRoutesWebAppBody = new StringBuilder();
        foreach (var classDeclarationSyntax in classes)
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
                var methodName = $"MapGet{classDeclarationSyntax.Identifier}";
                var methodBuilder = GetMethodBuilderByType("Get", classDeclarationSyntax, getAttributeSyntax, compilation, attributes, routeUsings);
                if (methodBuilder is not null)
                {
                    useRoutesBody.AppendLine($"{methodName}(builder);");
                    useRoutesWebAppBody.AppendLine($"{Constants.EndpointRouteBuilderExtensionsName}.{methodName}(builder);");
                    extensionClassBuilder.WithMethod(methodBuilder);
                }
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.PostAttributeName || a.Name.ToString() == Constants.PostAttributeShortName) is AttributeSyntax postAttributeSyntax)
            {
                var methodName = $"MapPost{classDeclarationSyntax.Identifier}";
                var methodBuilder = GetMethodBuilderByType("Post", classDeclarationSyntax, postAttributeSyntax, compilation, attributes, routeUsings);
                if (methodBuilder is not null)
                {
                    useRoutesBody.AppendLine($"{methodName}(builder);");
                    useRoutesWebAppBody.AppendLine($"{Constants.EndpointRouteBuilderExtensionsName}.{methodName}(builder);");
                    extensionClassBuilder.WithMethod(methodBuilder);
                }
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.PutAttributeName || a.Name.ToString() == Constants.PutAttributeShortName) is AttributeSyntax putAttributeSyntax)
            {
                var methodName = $"MapPut{classDeclarationSyntax.Identifier}";
                var methodBuilder = GetMethodBuilderByType("Put", classDeclarationSyntax, putAttributeSyntax, compilation, attributes, routeUsings);
                if (methodBuilder is not null)
                {
                    useRoutesBody.AppendLine($"{methodName}(builder);");
                    useRoutesWebAppBody.AppendLine($"{Constants.EndpointRouteBuilderExtensionsName}.{methodName}(builder);");
                    extensionClassBuilder.WithMethod(methodBuilder);
                }
            }
            else if (attributes.FirstOrDefault(a => a.Name.ToString() == Constants.DeleteAttributeName || a.Name.ToString() == Constants.DeleteAttributeShortName) is AttributeSyntax deleteAttributeSyntax)
            {
                var methodName = $"MapDelete{classDeclarationSyntax.Identifier}";
                var methodBuilder = GetMethodBuilderByType("Delete", classDeclarationSyntax, deleteAttributeSyntax, compilation, attributes, routeUsings);
                if (methodBuilder is not null)
                {
                    useRoutesBody.AppendLine($"{methodName}(builder);");
                    useRoutesWebAppBody.AppendLine($"{Constants.EndpointRouteBuilderExtensionsName}.{methodName}(builder);");
                    extensionClassBuilder.WithMethod(methodBuilder);
                }
            }
        }

        useRoutesBody.AppendLine("return builder;");
        useRoutesWebAppBody.AppendLine($"return builder;");
        useRoutesMethodBuilder.WithBody(useRoutesBody.ToString());
        useRoutesWebAppMethodBuilder.WithBody(useRoutesWebAppBody.ToString());
        extensionClassBuilder.WithMethod(useRoutesMethodBuilder);
        webAppBuilder.WithMethod(useRoutesWebAppMethodBuilder);
        foreach (var classUsing in routeUsings)
        {
            builder.WithUsing(classUsing);
        }

        var fileSource = builder.Build().ToFullString();
        sourceProductionContext.AddSource($"{Constants.EndpointRouteBuilderExtensionsName}.g", fileSource);
    }

    private static MethodBuilder? GetMethodBuilderByType(string type, ClassDeclarationSyntax classDeclarationSyntax, AttributeSyntax mapAttributeSyntax, Compilation compilation, List<AttributeSyntax> attributes, HashSet<string> usings)
    {
        var methodBuilder = SyntaxBuilder.CreateMethod(Constants.IEndpointRouteBuilderTypeName, $"Map{type}{classDeclarationSyntax.Identifier}")
                    .WithThisParameter(Constants.IEndpointRouteBuilderTypeName, Constants.BuilderParameterName)
                    .WithModifiers($"{Constants.Public} {Constants.Static}");

        var pattern = mapAttributeSyntax.ArgumentList?.Arguments.OfType<AttributeArgumentSyntax>().FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == Constants.PatternPropertyName)?.Expression.ToString().Trim('"');
        if (pattern is null)
        {
            return default;
        }

        var routeFilterSb = new StringBuilder();
        foreach(var attribute in attributes.Where(a => a.Name.ToString() == Constants.RouteFilterAttributeName || a.Name.ToString() == Constants.RouteFilterAttributeShortName))
        {
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            if (GetRouteFilterTypeName(semanticModel, attribute) is not string routeFilterType)
            {
                continue;
            }

            var namespaceIndex = routeFilterType.LastIndexOf(".");
            if (namespaceIndex > 0)
            {
                var routeFilterNamespace = routeFilterType.Substring(0, namespaceIndex);
                routeFilterType = routeFilterType.Substring(namespaceIndex + 1);
                usings.Add(routeFilterNamespace);
            }

            routeFilterSb.Append('\n')
                .Append(@$".AddEndpointFilter<{routeFilterType}>()");
        }

        methodBuilder.WithBody(@$"
        builder.MapGet(""{pattern}"", async (HttpContext context, {classDeclarationSyntax.Identifier} route) =>
        {{
            var request = await route.PreProcess(context, context.RequestAborted);
            var response = await route.HandleRequest(request, context.RequestAborted);
            return response.GetResult();
        }}){routeFilterSb};

        return builder;");

        return methodBuilder;
    }

    public static string? GetRouteFilterTypeName(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        var routeFilterTypeArgument = attributeSyntax.ArgumentList?.Arguments
            .FirstOrDefault(arg => arg.NameEquals?.Name.Identifier.Text == Constants.RouteFilterTypePropertyName);

        if (routeFilterTypeArgument is null)
        {
            return default;
        }

        if (routeFilterTypeArgument.Expression is not TypeOfExpressionSyntax typeExpression)
        {
            return default;
        }

        var typeSymbol = semanticModel.GetTypeInfo(typeExpression.Type);
        if (typeSymbol.Type is INamedTypeSymbol namedTypeSymbol)
        {
            // Get the fully qualified name (namespace + type name)
            return namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return default;
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

    private static void GenerateRouteFilterAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        var builder = SyntaxBuilder.CreateCompilationUnit()
                .WithNamespace(
                SyntaxBuilder.CreateNamespace(Constants.Namespace)
                    .WithClass(SyntaxBuilder.CreateClass(Constants.RouteFilterAttributeName)
                        .WithModifier(Constants.Public)
                        .WithConstructor(SyntaxBuilder.CreateConstructor(Constants.RouteFilterAttributeName)
                            .WithModifier(Constants.Public))
                        .WithAttribute(SyntaxBuilder.CreateAttribute("AttributeUsage")
                            .WithArgument(AttributeTargets.Class)
                            .WithArgument("Inherited", false)
                            .WithArgument("AllowMultiple", true))
                        .WithBaseClass(nameof(Attribute))
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.TypeType}?", Constants.RouteFilterTypePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter()))));


        var syntax = builder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"{Constants.RouteFilterAttributeName}.g", $"#nullable enable\n{source}\n#nullable disable\n");
    }
}

#nullable disable