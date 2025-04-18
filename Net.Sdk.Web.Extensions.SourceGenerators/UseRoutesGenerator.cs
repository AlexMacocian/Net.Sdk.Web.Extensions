﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sybil;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class UseRoutesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) => s is ClassDeclarationSyntax,
            transform: static (ctx, _) => GetFilteredClassDeclarationSyntax(ctx)).Where(static c => c is not null);
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());
        context.RegisterSourceOutput(compilationAndClasses, (sourceProductionContext, tuple) => Execute(tuple.Left, tuple.Right, sourceProductionContext));
    }

    private static (ClassDeclarationSyntax, List<MethodDeclarationSyntax>)? GetFilteredClassDeclarationSyntax(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (classDeclarationSyntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .OfType<AttributeSyntax>()
            .Any(s => s.Name.ToString() is Constants.RouteAttributeName or Constants.RouteAttributeShortName))
        {
            return (
                classDeclarationSyntax,
                classDeclarationSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.AttributeLists
                    .SelectMany(l => l.Attributes)
                    .OfType<AttributeSyntax>()
                    .Any(s => s.Name.ToString() is Constants.GetAttributeName 
                                                or Constants.GetAttributeShortName 
                                                or Constants.PostAttributeName
                                                or Constants.PostAttributeShortName
                                                or Constants.PutAttributeName
                                                or Constants.PutAttributeShortName
                                                or Constants.DeleteAttributeName
                                                or Constants.DeleteAttributeShortName))
                .ToList());
        }

        return default;
    }

    private static void Execute(Compilation compilation, ImmutableArray<(ClassDeclarationSyntax, List<MethodDeclarationSyntax>)?> classToMethodMapping, SourceProductionContext sourceProductionContext)
    {
        if (classToMethodMapping.IsDefaultOrEmpty)
        {
            return;
        }

        var maybeLanguageVersion = (compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions)?.LanguageVersion;
        if (!maybeLanguageVersion.HasValue)
        {
            return;
        }

        var languageVersion = maybeLanguageVersion.Value;
        var routeUsings = new HashSet<string>
        {
            Constants.UsingSystemThreading,
            Constants.UsingMicrosoftAspNetCoreRouting,
            Constants.UsingMicrosoftAspNetCoreHttp,
            Constants.UsingMicrosoftAspNetCoreBuilder,
            Constants.UsingMicrosoftAspNetCoreMvc,
            Constants.UsingSystemRuntimeCompilerServices
        };

        var builder = SyntaxBuilder.CreateCompilationUnit();
        var namespaceBuilder = languageVersion >= LanguageVersion.CSharp10 ? SyntaxBuilder.CreateFileScopedNamespace(Constants.Namespace) : SyntaxBuilder.CreateNamespace(Constants.Namespace);
        builder.WithNamespace(namespaceBuilder);
        var webAppBuilder = SyntaxBuilder.CreateClass(Constants.WebApplicationExtensionsName)
            .WithModifiers($"{Constants.Public} {Constants.Static}");
        namespaceBuilder.WithClass(webAppBuilder);
        var useRoutesWebAppMethodBuilder = SyntaxBuilder.CreateMethod(Constants.WebApplicationTypeName, Constants.UseRoutesMethodName)
                .WithThisParameter(Constants.WebApplicationTypeName, Constants.BuilderParameterName)
                .WithModifiers($"{Constants.Public} {Constants.Static}")
                .WithAttribute(SyntaxBuilder.CreateAttribute(Constants.MethodImplAttribute)
                    .WithRawArgument(Constants.MethodImplArgument));
        webAppBuilder.WithMethod(useRoutesWebAppMethodBuilder);

        var useRoutesWebAppBody = new StringBuilder();
        foreach (var classToMethodMap in classToMethodMapping)
        {
            if (classToMethodMap is null)
            {
                continue;
            }

            var classDeclarationSyntax = classToMethodMap.Value.Item1;
            var methodDeclarationSyntaxes = classToMethodMap.Value.Item2;
            if (methodDeclarationSyntaxes is null ||
                methodDeclarationSyntaxes.Count == 0)
            {
                continue;
            }

            if (GetParentOfType<BaseNamespaceDeclarationSyntax>(classDeclarationSyntax) is BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
            {
                routeUsings.Add(baseNamespaceDeclarationSyntax.Name.ToString());
                foreach(var u in baseNamespaceDeclarationSyntax.Usings)
                {
                    routeUsings.Add(u.NamespaceOrType.ToString());
                }
            }

            if (GetParentOfType<CompilationUnitSyntax>(classDeclarationSyntax) is CompilationUnitSyntax parentUnitSyntax)
            {
                foreach (var u in parentUnitSyntax.Usings)
                {
                    routeUsings.Add(u.NamespaceOrType.ToString());
                }
            }

            var classAttributes = classDeclarationSyntax.AttributeLists
                .SelectMany(l => l.Attributes)
                .OfType<AttributeSyntax>().ToList();
            var basePattern = classAttributes.FirstOrDefault(a => a.Name.ToString() == Constants.RouteAttributeName || a.Name.ToString() == Constants.RouteAttributeShortName)?
                .ArgumentList?.Arguments
                .FirstOrDefault()?
                .ToString().Trim('"');
            basePattern ??= string.Empty;
            foreach(var methodDeclarationSyntax in methodDeclarationSyntaxes)
            {
                var methodAttributes = methodDeclarationSyntax.AttributeLists
                    .SelectMany(l => l.Attributes)
                    .OfType<AttributeSyntax>().ToList();
                var pattern = methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.GetAttributeName 
                                                                                     or Constants.GetAttributeShortName
                                                                                     or Constants.PostAttributeName
                                                                                     or Constants.PostAttributeShortName
                                                                                     or Constants.PutAttributeName
                                                                                     or Constants.PutAttributeShortName
                                                                                     or Constants.DeleteAttributeName
                                                                                     or Constants.DeleteAttributeShortName)
                                              .ArgumentList?.Arguments
                                              .FirstOrDefault()?
                                              .ToString().Trim('"');

                if (string.IsNullOrWhiteSpace(basePattern) &&
                    string.IsNullOrWhiteSpace(pattern))
                {
                    continue;
                }

                var outerPattern = $"/{basePattern.Trim('/')}";
                var innerPattern = $"/{pattern?.Trim('/')}";
                var finalPattern = $"{outerPattern}{innerPattern}".Replace("//", "/");

                if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.GetAttributeName or Constants.GetAttributeShortName) is not null &&
                    GetMethodBodyByType("Get", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings) is string getMethodBody)
                {
                    useRoutesWebAppBody.AppendLine(getMethodBody);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.PostAttributeName or Constants.PostAttributeShortName) is not null &&
                    GetMethodBodyByType("Post", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings) is string postMethodBody)
                {
                    useRoutesWebAppBody.AppendLine(postMethodBody);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.PutAttributeName or Constants.PutAttributeShortName) is not null &&
                    GetMethodBodyByType("Put", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings) is string putMethodBoty)
                {
                    useRoutesWebAppBody.AppendLine(putMethodBoty);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.DeleteAttributeName or Constants.DeleteAttributeShortName) is not null &&
                    GetMethodBodyByType("Delete", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings) is string deleteMethodBody)
                {
                    useRoutesWebAppBody.AppendLine(deleteMethodBody);
                }
            }
        }

        useRoutesWebAppBody.AppendLine("return builder;");
        useRoutesWebAppMethodBuilder.WithBody(useRoutesWebAppBody.ToString());
        foreach (var classUsing in routeUsings)
        {
            builder.WithUsing(classUsing);
        }

        var fileSource = builder.Build().ToFullString();
        sourceProductionContext.AddSource($"{Constants.EndpointRouteBuilderExtensionsName}.g", fileSource);
    }

    private static string? GetMethodBodyByType(
        string type,
        string pattern,
        ClassDeclarationSyntax classDeclarationSyntax,
        MethodDeclarationSyntax methodDeclarationSyntax,
        Compilation compilation,
        HashSet<string> usings)
    {
        // Go over all attributes on the method and parent class and find all RouteFilters
        var semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
        var routeFilterSb = new StringBuilder();
        foreach(var attribute in methodDeclarationSyntax.AttributeLists
            .Concat(classDeclarationSyntax.AttributeLists)
            .SelectMany(a => a.Attributes)
            .OfType<AttributeSyntax>()
            .Where(attribute =>
            {
                var attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                if (attributeSymbol?.ContainingType == null)
                {
                    return false;
                }

                var attributeType = attributeSymbol.ContainingType;
                if (attributeType.Name is not Constants.RouteFilterAttributeName or Constants.RouteFilterAttributeShortName)
                {
                    return false;
                }

                return true;
            }))
        {
            if (attribute.Name is not GenericNameSyntax genericNameSyntax || genericNameSyntax.TypeArgumentList.Arguments.Count != 1)
            {
                continue;
            }

            var routeFilterTypeSyntax = genericNameSyntax.TypeArgumentList.Arguments[0];
            if (semanticModel.GetSymbolInfo(routeFilterTypeSyntax).Symbol is not ITypeSymbol routeFilterTypeSymbol)
            {
                continue;
            }

            var routeFilterType = routeFilterTypeSymbol.ToDisplayString();
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

        /*
         * Get the parameters of the route method. Ignore CancellationToken cancellationToken and HttpContext httpContext.
         * Those 2 will be automatically created by the generator.
         * Simply let the user request them in the method signature and they should flow downstream.
         */
        var parameters = GetMethodParameters(methodDeclarationSyntax);
        var variables = string.Join(", ", methodDeclarationSyntax.ParameterList.Parameters
            .Select(param => param.Identifier.Text));

        /*
         * Generate an async lambda for Task<IResult> or a synchronous lambda for IResult returns. If neither are matched, do not generate any method
         */
        var returnTypeSymbol = semanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType).Symbol as ITypeSymbol;
        return returnTypeSymbol?.ToDisplayString() switch
        {
            "System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>" => @$"
        builder.Map{type}(""{pattern}"", async (HttpContext httpContext, {classDeclarationSyntax.Identifier} route{(parameters.Length > 0 ? $", {parameters}" : "")}) =>
        {{
            var cancellationToken = httpContext.RequestAborted;
            return await route.{methodDeclarationSyntax.Identifier}({variables});
        }}){routeFilterSb};",
            "Microsoft.AspNetCore.Http.IResult" => @$"
        builder.Map{type}(""{pattern}"", (HttpContext httpContext, {classDeclarationSyntax.Identifier} route{(parameters.Length > 0 ? $", {parameters}" : "")}) =>
        {{
            var cancellationToken = httpContext.RequestAborted;
            return route.{methodDeclarationSyntax.Identifier}({variables});
        }}){routeFilterSb};",
            "System.Threading.Tasks.ValueTask<Microsoft.AspNetCore.Http.IResult>" => @$"
        builder.Map{type}(""{pattern}"", (HttpContext httpContext, {classDeclarationSyntax.Identifier} route{(parameters.Length > 0 ? $", {parameters}" : "")}) =>
        {{
            var cancellationToken = httpContext.RequestAborted;
            return route.{methodDeclarationSyntax.Identifier}({variables});
        }}){routeFilterSb};",
            _ => default
        };
    }

    private static string GetMethodParameters(MethodDeclarationSyntax methodDeclaration)
    {
        var parameters = methodDeclaration.ParameterList.Parameters
            .Where(param => param.Identifier.Text is not "httpContext" and not "cancellationToken")
            .Select(param =>
            {
                var parameterType = param.Type?.ToString();
                var parameterName = param.Identifier.Text;
                var attributes = param.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Select(attr => $"[{attr}]");

                var attributesString = string.Join(" ", attributes);
                return $"{attributesString} {parameterType} {parameterName}".Trim();
            });

        var parametersString = string.Join(", ", parameters);
        return parametersString;
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
}

#nullable disable