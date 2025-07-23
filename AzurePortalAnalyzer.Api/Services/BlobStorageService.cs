using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzurePortalAnalyzer.Api.Services;

public interface IBlobStorageService
{
    Task<string> UploadDocumentAsync(string fileName, Stream fileStream, string contentType);
    Task<Stream> DownloadDocumentAsync(string blobPath);
    Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan? expiry = null);
    Task DeleteDocumentAsync(string blobPath);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AzureStorage:ContainerName"] ?? "documents";
        _logger = logger;
    }

    public async Task<string> UploadDocumentAsync(string fileName, Stream fileStream, string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            _logger.LogInformation("Successfully uploaded document: {BlobName}", blobName);
            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadDocumentAsync(string blobPath)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {BlobPath}", blobPath);
            throw;
        }
    }

    public async Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan? expiry = null)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobPath,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
                };
                sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);

                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for: {BlobPath}", blobPath);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string blobPath)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Successfully deleted document: {BlobPath}", blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {BlobPath}", blobPath);
            throw;
        }
    }
}