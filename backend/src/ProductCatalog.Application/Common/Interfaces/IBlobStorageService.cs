namespace ProductCatalog.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string blobUrl, CancellationToken cancellationToken = default);
}
