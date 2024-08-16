using Azure.Core;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage;
using Net.Sdk.Web.Extensions.Azure.Options;

namespace Net.Sdk.Web.Extensions.Azure;

public class NamedBlobContainerClient<TCategory> : BlobContainerClient
{
    public NamedBlobContainerClient(string connectionString, string blobContainerName) : base(connectionString, blobContainerName)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, BlobClientOptions? options = null) : base(blobContainerUri, options)
    {
    }

    public NamedBlobContainerClient(string connectionString, string blobContainerName, BlobClientOptions options) : base(connectionString, blobContainerName, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, StorageSharedKeyCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, AzureSasCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    public NamedBlobContainerClient(Uri blobContainerUri, TokenCredential credential, BlobClientOptions? options = null) : base(blobContainerUri, credential, options)
    {
    }

    protected NamedBlobContainerClient()
    {
    }
}
