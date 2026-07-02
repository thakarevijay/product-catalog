namespace ProductCatalog.Infrastructure.Services;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Application.Common.Interfaces;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var accountUrl = configuration["BlobStorage:AccountUrl"]
            ?? throw new InvalidOperationException("BlobStorage:AccountUrl not configured");

        var containerName = configuration["BlobStorage:ContainerName"]
            ?? "product-images";

        // Uses Managed Identity in Azure, az login locally
        var blobServiceClient = new BlobServiceClient(
            new Uri(accountUrl),
            new DefaultAzureCredential());

        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Generate unique blob name
        var blobName = $"products/{Guid.NewGuid()}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Upload with content type
        await blobClient.UploadAsync(imageStream, new BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);

        // Return public URL
        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string blobUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(blobUrl);
        var blobName = uri.AbsolutePath.TrimStart('/').Replace("product-images/", "");
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
