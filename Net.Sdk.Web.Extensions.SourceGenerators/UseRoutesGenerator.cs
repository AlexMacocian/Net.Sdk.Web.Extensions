using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sybil;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

        var openApiSupport = false;
        if (compilation.GetTypeByMetadataName(Constants.ProducesResponseTypeType) is not null &&
            compilation.GetTypeByMetadataName(Constants.OpenApiConvetionBuilderExtensionsType) is not null)
        {
            openApiSupport = true;
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
                .WithModifiers($"{Constants.Public} {Constants.Static}");
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
                    GetMethodBodyByType("Get", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings, openApiSupport) is string getMethodBody)
                {
                    useRoutesWebAppBody.AppendLine(getMethodBody);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.PostAttributeName or Constants.PostAttributeShortName) is not null &&
                    GetMethodBodyByType("Post", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings, openApiSupport) is string postMethodBody)
                {
                    useRoutesWebAppBody.AppendLine(postMethodBody);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.PutAttributeName or Constants.PutAttributeShortName) is not null &&
                    GetMethodBodyByType("Put", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings, openApiSupport) is string putMethodBoty)
                {
                    useRoutesWebAppBody.AppendLine(putMethodBoty);
                }
                else if (methodAttributes.FirstOrDefault(a => a.Name.ToString() is Constants.DeleteAttributeName or Constants.DeleteAttributeShortName) is not null &&
                    GetMethodBodyByType("Delete", finalPattern, classDeclarationSyntax, methodDeclarationSyntax, compilation, routeUsings, openApiSupport) is string deleteMethodBody)
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
        HashSet<string> usings,
        bool openApiSupport)
    {
        // Go over all attributes on the method and parent class and find all RouteFilters
        var semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
        var extensionsBuilder = new StringBuilder();

        foreach (var attribute in methodDeclarationSyntax.AttributeLists
            .Concat(classDeclarationSyntax.AttributeLists)
            .SelectMany(a => a.Attributes)
            .OfType<AttributeSyntax>())
        {
            var attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
            AddRouteFilters(semanticModel, attribute, attributeSymbol, extensionsBuilder, usings);
            if (openApiSupport)
            {
                AddAttributeExtension(
                    attribute,
                    attributeSymbol,
                    extensionsBuilder,
                    Constants.EndpointNameAttributeName,
                    Constants.EndpointNameAttributeShortName,
                    Constants.WithNameExtension);

                AddAttributeExtension(
                    attribute,
                    attributeSymbol,
                    extensionsBuilder,
                    Constants.EndpointSummaryAttributeName,
                    Constants.EndpointSummaryAttributeShortName,
                    Constants.WithSummaryExtension);

                AddAttributeExtension(
                    attribute,
                    attributeSymbol,
                    extensionsBuilder,
                    Constants.EndpointDescriptionAttributeName,
                    Constants.EndpointDescriptionAttributeShortName,
                    Constants.WithDescriptionExtension);

                AddAuthorizeExtension(attribute, attributeSymbol, extensionsBuilder);
            }
        }

        if (openApiSupport)
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            AddProducesResponseTypeExtension(methodSymbol, extensionsBuilder);
            extensionsBuilder.AppendLine($"\n.{Constants.WithOpenApiExtension}()");
        }

        /*
         * Get the parameters of the route method. Ignore CancellationToken cancellationToken and HttpContext httpContext.
         * Those 2 will be automatically created by the generator.
         * Simply let the user request them in the method signature and they should flow downstream.
         */
        var parameters = GetMethodParameters(methodDeclarationSyntax);
        var variables = string.Join(", ", methodDeclarationSyntax.ParameterList.Parameters
            .Select(param => param.Identifier.Text));

        var newMethodName = $"{classDeclarationSyntax.Identifier}{methodDeclarationSyntax.Identifier}";
        /*
         * Generate an async lambda for Task<IResult> or a synchronous lambda for IResult returns. If neither are matched, do not generate any method
         */
        var returnTypeSymbol = semanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType).Symbol as ITypeSymbol;
        var returnType = returnTypeSymbol?.ToDisplayString();
        var isAsync = returnType switch
        {
            "System.Threading.Tasks.Task<Microsoft.AspNetCore.Http.IResult>" => (bool?)true,
            "System.Threading.Tasks.ValueTask<Microsoft.AspNetCore.Http.IResult>" => (bool?)true,
            "Microsoft.AspNetCore.Http.IResult" => (bool?)false,
            _ => default
        };

        if (isAsync is null)
        {
            return default;
        }

        var lambdaBody = @$"
        builder.Map{type}(""{pattern}"", static (HttpContext httpContext, {classDeclarationSyntax.Identifier} route{(parameters.Length > 0 ? $", {parameters}" : "")}) =>
        {{
            var cancellationToken = httpContext.RequestAborted;
            return {(isAsync is true ? $"new DeferredResult(route.{methodDeclarationSyntax.Identifier}({variables}))" : $"route.{methodDeclarationSyntax.Identifier}({variables})")};
        }})
        {extensionsBuilder};";
        return lambdaBody;
    }

    private static void AddProducesResponseTypeExtension(
        IMethodSymbol? methodSymbol,
        StringBuilder extensionsBuilder)
    {
        foreach (var attributeData in methodSymbol?.GetAttributes() ?? [])
        {
            var attrClassName = attributeData.AttributeClass?.Name;
            if (attrClassName is not Constants.ProducesResponseTypeAttributeName or Constants.ProducesResponseTypeAttributeShortName)
            {
                continue;
            }

            var typeArg = (attributeData.NamedArguments.FirstOrDefault(na => na.Key == "Type").Value.Value as ITypeSymbol)?.ToString();
            if (attributeData.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax attributeSyntax &&
                attributeSyntax.Name is GenericNameSyntax genericNameSyntax &&
                genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
            {
                typeArg = genericNameSyntax.TypeArgumentList.Arguments[0].ToString();
            }

            var statusCodeArg = attributeData.NamedArguments.FirstOrDefault(na => na.Key == "StatusCode").Value.Value as int? ?? 200;
            if (typeArg is null)
            {
                extensionsBuilder.Append($"\n.{Constants.ProducesAttributeShortName}(statusCode: {statusCodeArg})");
            }
            else
            {
                extensionsBuilder.Append($"\n.{Constants.ProducesAttributeShortName}(statusCode: {statusCodeArg}, responseType: typeof({typeArg}))");
            }
        }
    }

    private static void AddAuthorizeExtension(
        AttributeSyntax attributeSyntax,
        IMethodSymbol? attributeSymbol,
        StringBuilder extensionsBuilder)
    {
        if (attributeSymbol?.ContainingType is not INamedTypeSymbol attributeType ||
            attributeType.Name is not Constants.AuthorizeAttributeName or Constants.AuthorizeAttributeShortName)
        {
            return;
        }

        if (attributeSyntax.ArgumentList?.Arguments.Count > 0)
        {
            var policy = attributeSyntax.ArgumentList.Arguments[0].ToString().Trim('"');
            extensionsBuilder.Append($"\n.{Constants.RequiresAuthorization}(\"{policy}\")");
        }
        else
        {
            extensionsBuilder.Append($"\n.{Constants.RequiresAuthorization}()");
        }
    }

    private static void AddAttributeExtension(
        AttributeSyntax attributeSyntax,
        IMethodSymbol? attributeSymbol,
        StringBuilder extensionsBuilder,
        string attributeName,
        string attributeShortName,
        string extensionMethod)
    {
        if (attributeSymbol?.ContainingType is not INamedTypeSymbol attributeType ||
            (attributeType.Name != attributeName && attributeType.Name != attributeShortName))
        {
            return;
        }

        if (attributeSyntax.ArgumentList is null ||
            attributeSyntax.ArgumentList.Arguments.Count <= 0)
        {
            return;
        }

        var name = attributeSyntax.ArgumentList.Arguments[0].ToString().Trim('"');
        extensionsBuilder.Append($"\n.{extensionMethod}(\"{name}\")");
    }

    private static void AddRouteFilters(
        SemanticModel semanticModel,
        AttributeSyntax attributeSyntax,
        IMethodSymbol? attributeSymbol,
        StringBuilder extensionsBuilder,
        HashSet<string> usings)
    {
        if (attributeSymbol?.ContainingType is not INamedTypeSymbol attributeType ||
            attributeType.Name is not Constants.RouteFilterAttributeName or Constants.RouteFilterAttributeShortName ||
            attributeSyntax.Name is not GenericNameSyntax genericNameSyntax || genericNameSyntax.TypeArgumentList.Arguments.Count != 1)
        {
            return;
        }

        var routeFilterTypeSyntax = genericNameSyntax.TypeArgumentList.Arguments[0];
        if (semanticModel.GetSymbolInfo(routeFilterTypeSyntax).Symbol is not ITypeSymbol routeFilterTypeSymbol)
        {
            return;
        }

        var routeFilterType = routeFilterTypeSymbol.ToDisplayString();
        var namespaceIndex = routeFilterType.LastIndexOf(".");
        if (namespaceIndex > 0)
        {
            var routeFilterNamespace = routeFilterType.Substring(0, namespaceIndex);
            routeFilterType = routeFilterType.Substring(namespaceIndex + 1);
            usings.Add(routeFilterNamespace);
        }

        extensionsBuilder.Append(@$"\n.AddEndpointFilter<{routeFilterType}>()");
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