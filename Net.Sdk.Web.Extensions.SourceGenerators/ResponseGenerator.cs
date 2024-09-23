using Microsoft.CodeAnalysis;
using Sybil;

namespace Net.Sdk.Web.Extensions.SourceGenerators;

[Generator(LanguageNames.CSharp)]
#nullable enable
public class ResponseGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            GenerateResponse(context);
        });
    }

    private static void GenerateResponse(IncrementalGeneratorPostInitializationContext context)
    {
        var responseBaseClassBuilder = SyntaxBuilder.CreateCompilationUnit()
                .WithUsing(Constants.UsingMicrosoftAspNetCoreHttp)
                .WithNamespace(SyntaxBuilder.CreateNamespace(Constants.Namespace)
                    .WithClass(SyntaxBuilder.CreateClass(Constants.ResponseTypeName)
                        .WithModifiers($"{Constants.Public} {Constants.Abstract}")
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.IResultTypeName, Constants.GetResult)
                            .WithModifier(Constants.Public)
                            .WithBody(
                            @"return this switch
        {
            SuccessResponse success => Results.StatusCode(success.StatusCode),
            FailureResponse failure => Results.Problem(
                detail: string.IsNullOrWhiteSpace(failure.ErrorMessage) is false ? failure.ErrorMessage : default,
                statusCode: failure.StatusCode),
            ContentResponse content => Results.Content(content.ContentString, content.ContentType),
            JsonResponse json => Results.Ok(json.Value),
            _ => throw new InvalidOperationException($""Unexpected response type {this.GetType().Name}"")
        };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.SuccessResponseTypeName, Constants.OkSuccessResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithExpression("new() { StatusCode = 200 };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.SuccessResponseTypeName, Constants.CreatedSuccessResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithExpression("new() { StatusCode = 201 };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.ContentResponseTypeName, Constants.ContentContentResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ContentParameterName)
                            .WithParameter($"{Constants.StringType}?", Constants.ContentTypeParameterName, Constants.Default)
                            .WithExpression("new() { ContentString = content, ContentType = contentType };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.JsonResponseTypeName, Constants.JsonJsonResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.ObjectType, Constants.ValueParameterName)
                            .WithExpression("new() { Value = value };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.ContentResponseTypeName, Constants.PageContentResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ContentParameterName)
                            .WithExpression(@"new() { ContentString = content, ContentType = ""text/html"" };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.FailureResponseTypeName, Constants.BadRequestFailureResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ErrorMessageParameterName)
                            .WithExpression(@"new() { StatusCode = 400, ErrorMessage = errorMessage };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.FailureResponseTypeName, Constants.UnauthorizedFailureResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ErrorMessageParameterName)
                            .WithExpression(@"new() { StatusCode = 401, ErrorMessage = errorMessage };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.FailureResponseTypeName, Constants.ForbiddenFailureResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ErrorMessageParameterName)
                            .WithExpression(@"new() { StatusCode = 403, ErrorMessage = errorMessage };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.FailureResponseTypeName, Constants.NotFoundFailureResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ErrorMessageParameterName)
                            .WithExpression(@"new() { StatusCode = 404, ErrorMessage = errorMessage };"))
                        .WithMethod(SyntaxBuilder.CreateMethod(Constants.FailureResponseTypeName, Constants.ServerErrorFailureResponse)
                            .WithModifiers($"{Constants.Public} {Constants.Static}")
                            .WithParameter(Constants.StringType, Constants.ErrorMessageParameterName)
                            .WithExpression(@"new() { StatusCode = 500, ErrorMessage = errorMessage };")))
                    .WithClass(SyntaxBuilder.CreateClass(Constants.SuccessResponseTypeName)
                        .WithModifiers($"{Constants.Public} {Constants.Sealed}")
                        .WithBaseClass(Constants.ResponseTypeName)
                        .WithProperty(SyntaxBuilder.CreateProperty(Constants.IntType, Constants.StatusCodePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter())))
                    .WithClass(SyntaxBuilder.CreateClass(Constants.FailureResponseTypeName)
                        .WithModifiers($"{Constants.Public} {Constants.Sealed}")
                        .WithBaseClass(Constants.ResponseTypeName)
                        .WithProperty(SyntaxBuilder.CreateProperty(Constants.IntType, Constants.StatusCodePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter()))
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.StringType}?", Constants.ErrorMessagePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter())))
                    .WithClass(SyntaxBuilder.CreateClass(Constants.ContentResponseTypeName)
                        .WithModifiers($"{Constants.Public} {Constants.Sealed}")
                        .WithBaseClass(Constants.ResponseTypeName)
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.StringType}?", Constants.ContentStringPropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter()))
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.StringType}?", Constants.ContentTypePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter())))
                    .WithClass(SyntaxBuilder.CreateClass(Constants.JsonResponseTypeName)
                        .WithModifiers($"{Constants.Public} {Constants.Sealed}")
                        .WithBaseClass(Constants.ResponseTypeName)
                        .WithProperty(SyntaxBuilder.CreateProperty($"{Constants.ObjectType}?", Constants.ValuePropertyName)
                            .WithModifier(Constants.Public)
                            .WithAccessor(SyntaxBuilder.CreateGetter())
                            .WithAccessor(SyntaxBuilder.CreateSetter())))
                    );

        var syntax = responseBaseClassBuilder.Build();
        var source = syntax.ToFullString();
        context.AddSource($"Response.g", $"#nullable enable\n{source}\n#nullable disable\n");
    }
}

#nullable disable