using Microsoft.EntityFrameworkCore;
using AzurePortalAnalyzer.Api.Data;
using AzurePortalAnalyzer.Api.Models;

namespace AzurePortalAnalyzer.Api.Services;

public interface IPortalAnalysisService
{
    Task<PortalAnalysis> CreateAnalysisAsync(CreatePortalAnalysisRequest request, string userId);
    Task<PortalAnalysis?> GetAnalysisAsync(int id, string userId);
    Task<List<PortalAnalysis>> GetUserAnalysesAsync(string userId);
    Task<PortalAnalysis> UpdateAnalysisAsync(int id, CreatePortalAnalysisRequest request, string userId);
    Task<bool> DeleteAnalysisAsync(int id, string userId);
    Task<DocumentGenerationResult> GenerateDocumentAsync(GenerateDocumentRequest request, string userId);
    Task<RequirementDocument?> GetDocumentAsync(int documentId, string userId);
}

public class PortalAnalysisService : IPortalAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentGenerationService _documentGenerationService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<PortalAnalysisService> _logger;

    public PortalAnalysisService(
        ApplicationDbContext context,
        IDocumentGenerationService documentGenerationService,
        IBlobStorageService blobStorageService,
        ILogger<PortalAnalysisService> logger)
    {
        _context = context;
        _documentGenerationService = documentGenerationService;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<PortalAnalysis> CreateAnalysisAsync(CreatePortalAnalysisRequest request, string userId)
    {
        try
        {
            var analysis = new PortalAnalysis
            {
                Name = request.Name,
                Description = request.Description,
                UserId = userId,
                Components = request.Components.Select(c => new PortalComponent
                {
                    Name = c.Name,
                    Type = c.Type,
                    Description = c.Description,
                    Properties = c.Properties,
                    Requirements = c.Requirements
                }).ToList()
            };

            _context.PortalAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created portal analysis {AnalysisId} for user {UserId}", analysis.Id, userId);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portal analysis for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PortalAnalysis?> GetAnalysisAsync(int id, string userId)
    {
        return await _context.PortalAnalyses
            .Include(a => a.Components)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
    }

    public async Task<List<PortalAnalysis>> GetUserAnalysesAsync(string userId)
    {
        return await _context.PortalAnalyses
            .Include(a => a.Components)
            .Include(a => a.Documents)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.UpdatedAt)
            .ToListAsync();
    }

    public async Task<PortalAnalysis> UpdateAnalysisAsync(int id, CreatePortalAnalysisRequest request, string userId)
    {
        var analysis = await GetAnalysisAsync(id, userId);
        if (analysis == null)
        {
            throw new InvalidOperationException("Analysis not found or access denied");
        }

        try
        {
            analysis.Name = request.Name;
            analysis.Description = request.Description;
            analysis.UpdatedAt = DateTime.UtcNow;

            // Remove existing components
            _context.PortalComponents.RemoveRange(analysis.Components);

            // Add updated components
            analysis.Components = request.Components.Select(c => new PortalComponent
            {
                Name = c.Name,
                Type = c.Type,
                Description = c.Description,
                Properties = c.Properties,
                Requirements = c.Requirements,
                PortalAnalysisId = analysis.Id
            }).ToList();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated portal analysis {AnalysisId} for user {UserId}", analysis.Id, userId);
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portal analysis {AnalysisId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAnalysisAsync(int id, string userId)
    {
        var analysis = await GetAnalysisAsync(id, userId);
        if (analysis == null)
        {
            return false;
        }

        try
        {
            // Delete associated documents from blob storage
            foreach (var document in analysis.Documents)
            {
                if (!string.IsNullOrEmpty(document.BlobPath))
                {
                    await _blobStorageService.DeleteDocumentAsync(document.BlobPath);
                }
            }

            _context.PortalAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted portal analysis {AnalysisId} for user {UserId}", id, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portal analysis {AnalysisId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<DocumentGenerationResult> GenerateDocumentAsync(GenerateDocumentRequest request, string userId)
    {
        var analysis = await GetAnalysisAsync(request.PortalAnalysisId, userId);
        if (analysis == null)
        {
            throw new InvalidOperationException("Analysis not found or access denied");
        }

        try
        {
            byte[]? documentData = null;
            string? documentContent = null;
            string contentType;
            string fileExtension;

            switch (request.Format.ToUpper())
            {
                case "PDF":
                    documentData = await _documentGenerationService.GeneratePdfAsync(analysis, request.Title, request.Description);
                    contentType = "application/pdf";
                    fileExtension = "pdf";
                    break;
                case "MARKDOWN":
                    documentContent = await _documentGenerationService.GenerateMarkdownAsync(analysis, request.Title, request.Description);
                    contentType = "text/markdown";
                    fileExtension = "md";
                    break;
                case "HTML":
                    documentContent = await _documentGenerationService.GenerateHtmlAsync(analysis, request.Title, request.Description);
                    contentType = "text/html";
                    fileExtension = "html";
                    break;
                default:
                    throw new ArgumentException($"Unsupported document format: {request.Format}");
            }

            // Upload to blob storage
            string fileName = $"{request.Title.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";
            Stream fileStream;

            if (documentData != null)
            {
                fileStream = new MemoryStream(documentData);
            }
            else if (documentContent != null)
            {
                fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(documentContent));
            }
            else
            {
                throw new InvalidOperationException("Failed to generate document content");
            }

            var blobPath = await _blobStorageService.UploadDocumentAsync(fileName, fileStream, contentType);
            fileStream.Dispose();

            // Save document record
            var document = new RequirementDocument
            {
                Title = request.Title,
                Description = request.Description,
                Format = request.Format.ToUpper(),
                BlobPath = blobPath,
                PortalAnalysisId = analysis.Id
            };

            _context.RequirementDocuments.Add(document);
            await _context.SaveChangesAsync();

            var downloadUrl = await _blobStorageService.GetDownloadUrlAsync(blobPath, TimeSpan.FromHours(24));

            _logger.LogInformation("Generated {Format} document {DocumentId} for analysis {AnalysisId}", 
                request.Format, document.Id, analysis.Id);

            return new DocumentGenerationResult
            {
                DocumentId = document.Id,
                Title = document.Title,
                Format = document.Format,
                DownloadUrl = downloadUrl,
                GeneratedAt = document.GeneratedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating document for analysis {AnalysisId}", request.PortalAnalysisId);
            throw;
        }
    }

    public async Task<RequirementDocument?> GetDocumentAsync(int documentId, string userId)
    {
        return await _context.RequirementDocuments
            .Include(d => d.PortalAnalysis)
            .FirstOrDefaultAsync(d => d.Id == documentId && d.PortalAnalysis.UserId == userId);
    }
}