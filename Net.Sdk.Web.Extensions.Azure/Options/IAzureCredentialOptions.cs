namespace Net.Sdk.Web.Extensions.Azure.Options;

public interface IAzureCredentialOptions
{
    string ClientId { get; set; }

    string TenantId { get; set; }
}
