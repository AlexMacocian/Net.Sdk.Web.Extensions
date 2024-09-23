using Microsoft.CodeAnalysis;
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
public class WithRoutesGenerator : IIncrementalGenerator
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

        var extensionClassBuilder = SyntaxBuilder.CreateClass(Constants.WebApplicationBuilderExtensionsName)
            .WithModifiers($"{Constants.Public} {Constants.Static}");
        namespaceBuilder.WithClass(extensionClassBuilder);

        var methodBuilder = SyntaxBuilder.CreateMethod(Constants.WebApplicationBuilderTypeName, Constants.WithRoutesName)
            .WithModifiers($"{Constants.Public} {Constants.Static}")
            .WithThisParameter(Constants.WebApplicationBuilderTypeName, Constants.BuilderParameterName);

        extensionClassBuilder.WithMethod(methodBuilder);
        var body = new StringBuilder();
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

            body.AppendLine($"builder.Services.AddScoped<{classDeclarationSyntax.Identifier}>();");
        }

        body.AppendLine("return builder;");
        methodBuilder.WithBody(body.ToString());
        foreach(var classUsing in routeUsings)
        {
            builder.WithUsing(classUsing);
        }

        var fileSource = builder.Build().ToFullString();
        sourceProductionContext.AddSource($"{Constants.WebApplicationBuilderExtensionsName}.g", fileSource);
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