using AzurePortalAnalyzer.Core.Models;

namespace AzurePortalAnalyzer.Core.Services;

public interface IDocumentService
{
    Task<Document> CreateDocumentAsync(string title, DocumentType type, string userId);
    Task<Document?> GetDocumentAsync(int id);
    Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId);
    Task<Document> UpdateDocumentStatusAsync(int id, DocumentStatus status);
    Task<bool> DeleteDocumentAsync(int id, string userId);
}

public interface IDocumentStorageService
{
    Task<string> UploadDocumentAsync(Stream content, string fileName, string contentType);
    Task<Stream> DownloadDocumentAsync(string blobName);
    Task<string> GenerateSecureDownloadUrlAsync(string blobName, TimeSpan expiry);
    Task<bool> DeleteDocumentAsync(string blobName);
}

public interface IDocumentGenerationService
{
    Task<byte[]> GeneratePdfReportAsync(object reportData, string templateName);
    Task<string> GenerateMarkdownReportAsync(object reportData, string templateName);
}

public class SecureDownloadRequest
{
    public int DocumentId { get; set; }
    public string UserId { get; set; } = string.Empty;
}