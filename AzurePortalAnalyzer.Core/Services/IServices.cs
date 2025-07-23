using AzurePortalAnalyzer.Core.Models;

namespace AzurePortalAnalyzer.Core.Services;

public interface IAzureAnalysisService
{
    Task<AnalysisResult> AnalyzeAzureResourcesAsync(AzureAnalysisRequest request);
    Task<AzureAnalysisRequest> GetRequestAsync(Guid requestId);
    Task<AzureAnalysisRequest> CreateRequestAsync(AzureAnalysisRequest request);
    Task UpdateRequestStatusAsync(Guid requestId, AnalysisStatus status, string? errorMessage = null);
}

public interface IDocumentGenerationService
{
    Task<byte[]> GeneratePdfDocumentAsync(AnalysisResult analysisResult);
    Task<string> GenerateMarkdownDocumentAsync(AnalysisResult analysisResult);
}

public interface IBlobStorageService
{
    Task<string> UploadDocumentAsync(byte[] documentContent, string fileName, string contentType);
    Task<byte[]> DownloadDocumentAsync(string blobUrl);
    Task DeleteDocumentAsync(string blobUrl);
}

public interface IEmailService
{
    Task SendDocumentAsync(string recipientEmail, string documentUrl, string fileName);
    Task SendAnalysisCompleteAsync(string recipientEmail, AnalysisResult result, string documentUrl);
}

public interface IAnalysisOrchestrationService
{
    Task ProcessAnalysisRequestAsync(Guid requestId);
}