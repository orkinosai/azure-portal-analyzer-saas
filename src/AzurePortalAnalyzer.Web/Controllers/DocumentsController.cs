using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AzurePortalAnalyzer.Core.Services;
using AzurePortalAnalyzer.Core.Models;

namespace AzurePortalAnalyzer.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentStorageService _storageService;
    private readonly IDocumentGenerationService _generationService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        IDocumentStorageService storageService,
        IDocumentGenerationService generationService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _storageService = storageService;
        _generationService = generationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Document>>> GetUserDocuments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var documents = await _documentService.GetUserDocumentsAsync(userId);
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Document>> GetDocument(int id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (document.UserId != userId)
        {
            return Forbid();
        }

        return Ok(document);
    }

    [HttpPost]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var document = await _documentService.CreateDocumentAsync(request.Title, request.Type, userId);
            
            // Start document generation asynchronously
            _ = Task.Run(async () => await GenerateDocumentAsync(document.Id));

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return BadRequest("Failed to create document");
        }
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (document.UserId != userId)
        {
            return Forbid();
        }

        if (document.Status != DocumentStatus.Generated)
        {
            return BadRequest("Document is not ready for download");
        }

        try
        {
            // Generate secure download URL with 1-hour expiry
            var downloadUrl = await _storageService.GenerateSecureDownloadUrlAsync(document.BlobName, TimeSpan.FromHours(1));
            
            return Ok(new { downloadUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for document {DocumentId}", id);
            return BadRequest("Failed to generate download link");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var document = await _documentService.GetDocumentAsync(id);
            if (document != null && !string.IsNullOrEmpty(document.BlobName))
            {
                await _storageService.DeleteDocumentAsync(document.BlobName);
            }

            var deleted = await _documentService.DeleteDocumentAsync(id, userId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return BadRequest("Failed to delete document");
        }
    }

    private async Task GenerateDocumentAsync(int documentId)
    {
        try
        {
            await _documentService.UpdateDocumentStatusAsync(documentId, DocumentStatus.Processing);

            var document = await _documentService.GetDocumentAsync(documentId);
            if (document == null) return;

            byte[] content;
            string contentType;
            string fileName;

            // Generate document based on type
            switch (document.Type)
            {
                case DocumentType.PortalAnalysisReport:
                    content = await _generationService.GeneratePdfReportAsync(new { }, "portal-analysis");
                    contentType = "application/pdf";
                    fileName = $"{document.FileName}.pdf";
                    break;
                default:
                    var markdownContent = await _generationService.GenerateMarkdownReportAsync(new { }, "default");
                    content = System.Text.Encoding.UTF8.GetBytes(markdownContent);
                    contentType = "text/markdown";
                    fileName = $"{document.FileName}.md";
                    break;
            }

            // Upload to blob storage
            using var stream = new MemoryStream(content);
            var blobName = await _storageService.UploadDocumentAsync(stream, fileName, contentType);

            // Update document with blob information
            document.BlobName = blobName;
            document.Size = content.Length;
            document.ContentType = contentType;
            document.FileName = fileName;

            await _documentService.UpdateDocumentStatusAsync(documentId, DocumentStatus.Generated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating document {DocumentId}", documentId);
            await _documentService.UpdateDocumentStatusAsync(documentId, DocumentStatus.Failed);
        }
    }
}

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
}