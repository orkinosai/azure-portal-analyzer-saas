using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzurePortalAnalyzer.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzurePortalAnalyzer.Core.Services.Implementations;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly string _containerName;

    public BlobStorageService(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        _containerName = configuration["Azure:BlobStorage:ContainerName"] ?? "documents";
    }

    public async Task<string> UploadDocumentAsync(byte[] documentContent, string fileName, string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}-{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(documentContent);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            
            await blobClient.UploadAsync(stream, new BlobUploadOptions 
            { 
                HttpHeaders = blobHttpHeaders 
            });

            _logger.LogInformation("Document uploaded successfully: {BlobName}", blobName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> DownloadDocumentAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Document deleted successfully: {BlobUrl}", blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {BlobUrl}", blobUrl);
            throw;
        }
    }
}