namespace Net.Sdk.Web.Extensions.Azure.Options;

public interface IAzureClientSecretCredentialOptions : IAzureCredentialOptions
{
    string ClientSecret { get; set; }
}