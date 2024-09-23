namespace Net.Sdk.Web.Extensions.SourceGenerators;

public static class Constants
{
    public const string Partial = "partial";
    public const string Public = "public";
    public const string Abstract = "abstract";
    public const string Sealed = "sealed";
    public const string Static = "static";
    public const string StringType = "string";
    public const string IntType = "int";
    public const string ObjectType = "object";
    public const string Default = "default";
    public const string Namespace = "Net.Sdk.Web";
    public const string UsingSystemThreading = "System.Threading";
    public const string UsingMicrosoftAspNetCoreHttp = "Microsoft.AspNetCore.Http";
    public const string UsingMicrosoftAspNetCoreRouting = "Microsoft.AspNetCore.Routing";
    public const string UsingMicrosoftAspNetCoreBuilder = "Microsoft.AspNetCore.Builder";

    public const string RouteFileName = "Route";
    public const string RouteInterface = "IRoute";
    public const string RouteTypeParameter = "TRequest";
    public const string PreProcessRequestReturnType = "Task<TRequest?>";
    public const string PreProcessRequestMethodName = "PreProcess";
    public const string HandleRequestReturnType = "Task<Response>";
    public const string HandleRequestMethodName = "HandleRequest";
    public const string HttpContextTypeName = "HttpContext?";
    public const string HttpContextParameterName = "context";
    public const string RequestParameterName = "request";

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

    public const string GetAttributeName = "GenerateMapGetAttribute";
    public const string GetAttributeShortName = "GenerateMapGet";
    public const string PostAttributeName = "GenerateMapPostAttribute";
    public const string PostAttributeShortName = "GenerateMapPost";
    public const string PutAttributeName = "GenerateMapPutAttribute";
    public const string PutAttributeShortName = "GenerateMapPut";
    public const string DeleteAttributeName = "GenerateMapDeleteAttribute";
    public const string DeleteAttributeShortName = "GenerateMapDelete";

    public const string PatternPropertyName = "Pattern";

    public const string EndpointRouteBuilderExtensionsName = "EndpointRouteBuilderExtensions";
    public const string IEndpointRouteBuilderTypeName = "IEndpointRouteBuilder";
    public const string BuilderParameterName = "builder";
}
