using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Net.Sdk.Web.Extensions.Azure.Options;
using System.Core.Extensions;
using System.Extensions;

namespace Net.Sdk.Web.Extensions.Azure;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureAzureClientSecretCredentials(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull()
            .ConfigureExtended<AzureCredentialsOptions>()
            .Services.AddSingleton<TokenCredential>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureCredentialsOptions>>().Value;
                return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
            });

        return builder;
    }

    public static WebApplicationBuilder ConfigureAzureClientSecretCredentials<TOptions>(this WebApplicationBuilder builder)
       where TOptions : class, IAzureClientSecretCredentialOptions, new()
    {
        builder.ThrowIfNull()
            .ConfigureExtended<TOptions>()
            .Services.AddSingleton<TokenCredential>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
                return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientSingleton<TCategory, TAccountOptions, TTableOptions>(this WebApplicationBuilder builder)
        where TTableOptions : class, IAzureTableStorageOptions, new()
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        builder.Services.ThrowIfNull()
            .AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TTableOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{storageOptions.Value.AccountName}.table.core.windows.net"), clientOptions.Value.TableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientSingleton<TCategory, TAccountOptions>(this WebApplicationBuilder builder, string tableName)
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        tableName.ThrowIfNull();
        builder.Services.ThrowIfNull()
            .AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{storageOptions.Value.AccountName}.table.core.windows.net"), tableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientSingleton<TCategory>(this WebApplicationBuilder builder, string accountName, string tableName)
    {
        tableName.ThrowIfNull();
        accountName.ThrowIfNull();
        builder.Services.ThrowIfNull()
            .AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{accountName}.table.core.windows.net"), tableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientScoped<TCategory, TAccountOptions, TTableOptions>(this WebApplicationBuilder builder)
        where TTableOptions : class, IAzureTableStorageOptions, new()
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        builder.Services.ThrowIfNull()
            .AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TTableOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{storageOptions.Value.AccountName}.table.core.windows.net"), clientOptions.Value.TableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientScoped<TCategory, TAccountOptions>(this WebApplicationBuilder builder, string tableName)
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        tableName.ThrowIfNull();
        builder.Services.ThrowIfNull()
            .AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{storageOptions.Value.AccountName}.table.core.windows.net"), tableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithTableClientScoped<TCategory>(this WebApplicationBuilder builder, string accountName, string tableName)
    {
        tableName.ThrowIfNull();
        accountName.ThrowIfNull();
        builder.Services.ThrowIfNull()
            .AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var logger = sp.GetRequiredService<ILogger<NamedTableClient<TCategory>>>();
                return new NamedTableClient<TCategory>(logger, new Uri($"https://{accountName}.table.core.windows.net"), tableName, tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientSingleton<TCategory, TAccountOptions, TBlobContainerOptions>(this WebApplicationBuilder builder)
        where TBlobContainerOptions : class, IAzureBlobStorageOptions, new()
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        builder.ThrowIfNull()
            .Services.AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TBlobContainerOptions>>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{storageOptions.Value.AccountName}.blob.core.windows.net/{clientOptions.Value.ContainerName}"), tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientSingleton<TCategory, TAccountOptions>(this WebApplicationBuilder builder, string containerName)
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        containerName.ThrowIfNull();
        builder.ThrowIfNull()
            .Services.AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{storageOptions.Value.AccountName}.blob.core.windows.net/{containerName}"), tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientSingleton<TCategory>(this WebApplicationBuilder builder, string accountName, string containerName)
    {
        accountName.ThrowIfNull();
        containerName.ThrowIfNull();
        builder.ThrowIfNull()
            .Services.AddSingleton(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"), tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientScoped<TCategory, TAccountOptions, TBlobContainerOptions>(this WebApplicationBuilder builder)
        where TBlobContainerOptions : class, IAzureBlobStorageOptions, new()
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        builder.ThrowIfNull()
            .Services.AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                var clientOptions = sp.GetRequiredService<IOptions<TBlobContainerOptions>>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{storageOptions.Value.AccountName}.blob.core.windows.net/{clientOptions.Value.ContainerName}"), tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientScoped<TCategory, TAccountOptions>(this WebApplicationBuilder builder, string containerName)
        where TAccountOptions : class, IStorageAccountOptions, new()
    {
        containerName.ThrowIfNull();
        builder.ThrowIfNull()
            .Services.AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                var storageOptions = sp.GetRequiredService<IOptions<TAccountOptions>>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{storageOptions.Value.AccountName}.blob.core.windows.net/{containerName}"), tokenCredential, default);
            });

        return builder;
    }

    public static WebApplicationBuilder WithBlobContainerClientScoped<TCategory>(this WebApplicationBuilder builder, string accountName, string containerName)
    {
        accountName.ThrowIfNull();
        containerName.ThrowIfNull();
        builder.ThrowIfNull()
            .Services.AddScoped(sp =>
            {
                var tokenCredential = sp.GetRequiredService<TokenCredential>();
                return new NamedBlobContainerClient<TCategory>(new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"), tokenCredential, default);
            });

        return builder;
    }
}
