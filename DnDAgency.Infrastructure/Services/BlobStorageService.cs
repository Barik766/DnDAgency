using Azure.Storage.Blobs;

namespace DnDAgency.Infrastructure.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _client;

    public BlobStorageService(string connectionString)
    {
        _client = new BlobServiceClient(connectionString);
    }

    public async Task UploadFileAsync(string containerName, string blobName, Stream content)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();
        var blob = container.GetBlobClient(blobName);
        await blob.UploadAsync(content, overwrite: true);
    }
}
