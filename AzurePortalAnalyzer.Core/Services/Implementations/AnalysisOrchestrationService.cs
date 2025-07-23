using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AzurePortalAnalyzer.Core.Services.Implementations;

public class AnalysisOrchestrationService : IAnalysisOrchestrationService
{
    private readonly IAzureAnalysisService _azureAnalysisService;
    private readonly IDocumentGenerationService _documentGenerationService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AnalysisOrchestrationService> _logger;

    public AnalysisOrchestrationService(
        IAzureAnalysisService azureAnalysisService,
        IDocumentGenerationService documentGenerationService,
        IBlobStorageService blobStorageService,
        IEmailService emailService,
        ILogger<AnalysisOrchestrationService> logger)
    {
        _azureAnalysisService = azureAnalysisService;
        _documentGenerationService = documentGenerationService;
        _blobStorageService = blobStorageService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcessAnalysisRequestAsync(Guid requestId)
    {
        AzureAnalysisRequest? request = null;
        
        try
        {
            _logger.LogInformation("Starting analysis processing for request {RequestId}", requestId);
            
            // Get the request
            request = await _azureAnalysisService.GetRequestAsync(requestId);
            
            // Update status to in progress
            await _azureAnalysisService.UpdateRequestStatusAsync(requestId, AnalysisStatus.InProgress);
            
            // Perform the analysis
            var analysisResult = await _azureAnalysisService.AnalyzeAzureResourcesAsync(request);
            
            // Generate PDF document
            var pdfDocument = await _documentGenerationService.GeneratePdfDocumentAsync(analysisResult);
            
            // Upload document to blob storage
            var fileName = $"azure-analysis-{requestId}.pdf";
            var documentUrl = await _blobStorageService.UploadDocumentAsync(pdfDocument, fileName, "application/pdf");
            
            // Update request with document URL
            request.DocumentUrl = documentUrl;
            await _azureAnalysisService.UpdateRequestStatusAsync(requestId, AnalysisStatus.Completed);
            
            // Send email with analysis results
            await _emailService.SendAnalysisCompleteAsync(request.UserEmail, analysisResult, documentUrl);
            
            // Update status to email sent
            await _azureAnalysisService.UpdateRequestStatusAsync(requestId, AnalysisStatus.EmailSent);
            
            _logger.LogInformation("Analysis processing completed successfully for request {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing analysis request {RequestId}", requestId);
            
            if (request != null)
            {
                await _azureAnalysisService.UpdateRequestStatusAsync(requestId, AnalysisStatus.Failed, ex.Message);
            }
            
            throw;
        }
    }
}