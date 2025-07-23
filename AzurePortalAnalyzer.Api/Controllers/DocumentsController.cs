using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AzurePortalAnalyzer.Api.Models;
using AzurePortalAnalyzer.Api.Services;

namespace AzurePortalAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IPortalAnalysisService _portalAnalysisService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IPortalAnalysisService portalAnalysisService,
        IBlobStorageService blobStorageService,
        ILogger<DocumentsController> logger)
    {
        _portalAnalysisService = portalAnalysisService;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpPost("generate")]
    public async Task<ActionResult<DocumentGenerationResult>> GenerateDocument([FromBody] GenerateDocumentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _portalAnalysisService.GenerateDocumentAsync(request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid document generation request");
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid document format requested");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating document");
            return StatusCode(500, "An error occurred while generating the document");
        }
    }

    [HttpGet("{documentId}")]
    public async Task<ActionResult<RequirementDocument>> GetDocument(int documentId)
    {
        try
        {
            var userId = GetUserId();
            var document = await _portalAnalysisService.GetDocumentAsync(documentId, userId);
            
            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while retrieving the document");
        }
    }

    [HttpGet("{documentId}/download")]
    public async Task<ActionResult> DownloadDocument(int documentId)
    {
        try
        {
            var userId = GetUserId();
            var document = await _portalAnalysisService.GetDocumentAsync(documentId, userId);
            
            if (document == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(document.BlobPath))
            {
                return NotFound("Document file not found");
            }

            var fileStream = await _blobStorageService.DownloadDocumentAsync(document.BlobPath);
            var fileName = $"{document.Title}_{document.GeneratedAt:yyyyMMdd}.{GetFileExtension(document.Format)}";
            
            return File(fileStream, GetContentType(document.Format), fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while downloading the document");
        }
    }

    [HttpGet("{documentId}/download-url")]
    public async Task<ActionResult<string>> GetDownloadUrl(int documentId, [FromQuery] int hoursValid = 1)
    {
        try
        {
            var userId = GetUserId();
            var document = await _portalAnalysisService.GetDocumentAsync(documentId, userId);
            
            if (document == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(document.BlobPath))
            {
                return NotFound("Document file not found");
            }

            var downloadUrl = await _blobStorageService.GetDownloadUrlAsync(document.BlobPath, TimeSpan.FromHours(hoursValid));
            return Ok(downloadUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for document {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while generating the download URL");
        }
    }

    private static string GetContentType(string format)
    {
        return format.ToUpper() switch
        {
            "PDF" => "application/pdf",
            "MARKDOWN" => "text/markdown",
            "HTML" => "text/html",
            _ => "application/octet-stream"
        };
    }

    private static string GetFileExtension(string format)
    {
        return format.ToUpper() switch
        {
            "PDF" => "pdf",
            "MARKDOWN" => "md",
            "HTML" => "html",
            _ => "txt"
        };
    }
}