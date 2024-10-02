namespace Net.Sdk.Web.Extensions.SourceGenerators;

public static class Constants
{
    public const string Partial = "partial";
    public const string Public = "public";
    public const string Abstract = "abstract";
    public const string Sealed = "sealed";
    public const string Static = "static";
    public const string StringType = "string";
    public const string TypeType = "Type";
    public const string IntType = "int";
    public const string ObjectType = "object";
    public const string Default = "default";
    public const string Namespace = "Net.Sdk.Web";
    public const string UsingSystemThreading = "System.Threading";
    public const string UsingMicrosoftAspNetCoreHttp = "Microsoft.AspNetCore.Http";
    public const string UsingMicrosoftAspNetCoreRouting = "Microsoft.AspNetCore.Routing";
    public const string UsingMicrosoftAspNetCoreBuilder = "Microsoft.AspNetCore.Builder";
    public const string UsingMicrosoftAspNetCoreMvc = "Microsoft.AspNetCore.Mvc";

    public const string RouteAttributeName = "GenerateControllerAttribute";
    public const string RouteAttributeShortName = "GenerateController";

    public const string ResponseTypeName = "Response";
    public const string IResultTypeName = "IResult";
    public const string SuccessResponseTypeName = "SuccessResponse";
    public const string FailureResponseTypeName = "FailureResponse";
    public const string ContentResponseTypeName = "ContentResponse";
    public const string JsonResponseTypeName = "JsonResponse";
    public const string StatusCodePropertyName = "StatusCode";
    public const string ErrorMessagePropertyName = "ErrorMessage";
    public const string ContentStringPropertyName = "ContentString";
    public const string ValuePropertyName = "Value";
    public const string ContentTypePropertyName = "ContentType";
    public const string OkSuccessResponse = "Ok";
    public const string CreatedSuccessResponse = "Created";
    public const string ContentContentResponse = "Content";
    public const string JsonJsonResponse = "Json";
    public const string PageContentResponse = "Page";
    public const string BadRequestFailureResponse = "BadRequest";
    public const string UnauthorizedFailureResponse = "Unauthorized";
    public const string ForbiddenFailureResponse = "Forbidden";
    public const string NotFoundFailureResponse = "NotFound";
    public const string ServerErrorFailureResponse = "ServerError";
    public const string GetResult = "GetResult";
    public const string ContentParameterName = "content";
    public const string ContentTypeParameterName = "contentType";
    public const string ValueParameterName = "value";
    public const string ErrorMessageParameterName = "errorMessage";

    public const string CancellationTokenTypeName = "CancellationToken";
    public const string CancellationTokenParameterName = "cancellationToken";

    public const string GetAttributeName = "GenerateGetAttribute";
    public const string GetAttributeShortName = "GenerateGet";
    public const string PostAttributeName = "GeneratePostAttribute";
    public const string PostAttributeShortName = "GeneratePost";
    public const string PutAttributeName = "GeneratePutAttribute";
    public const string PutAttributeShortName = "GeneratePut";
    public const string DeleteAttributeName = "GenerateDeleteAttribute";
    public const string DeleteAttributeShortName = "GenerateDelete";

    public const string PatternPropertyName = "Pattern";
    public const string PatternArgumentName = "pattern";

    public const string MapAllRoutes = "MapAllRoutes";
    public const string UseRoutesMethodName = "UseRoutes";
    public const string EndpointRouteBuilderExtensionsName = "EndpointRouteBuilderExtensions";
    public const string IEndpointRouteBuilderTypeName = "IEndpointRouteBuilder";
    public const string WebApplicationBuilderExtensionsName = "WebApplicationBuilderExtension";
    public const string WebApplicationBuilderTypeName = "WebApplicationBuilder";
    public const string WebApplicationExtensionsName = "WebApplicationExtensions";
    public const string WebApplicationTypeName = "WebApplication";
    public const string WithRoutesName = "WithRoutes";
    public const string BuilderParameterName = "builder";

    public const string RouteFilterAttributeName = "RouteFilterAttribute";
    public const string RouteFilterAttributeShortName = "RouteFilter";
    public const string RouteFilterTypePropertyName = "RouteFilterType";

    public const string T = "T";
    public const string IEndpointFilterName = "IEndpointFilter";
}
