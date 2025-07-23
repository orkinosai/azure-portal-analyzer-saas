using Microsoft.EntityFrameworkCore;
using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using AzurePortalAnalyzer.Infrastructure.Data;

namespace AzurePortalAnalyzer.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateDocumentAsync(string title, DocumentType type, string userId)
    {
        var document = new Document
        {
            Title = title,
            Type = type,
            UserId = userId,
            Status = DocumentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FileName = $"{title}_{DateTime.UtcNow:yyyyMMddHHmmss}",
            BlobName = string.Empty,
            ContentType = type == DocumentType.PortalAnalysisReport ? "application/pdf" : "text/markdown",
            Size = 0
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<Document?> GetDocumentAsync(int id)
    {
        return await _context.Documents.FindAsync(id);
    }

    public async Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId)
    {
        return await _context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Document> UpdateDocumentStatusAsync(int id, DocumentStatus status)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            throw new ArgumentException($"Document with ID {id} not found.");
        }

        document.Status = status;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int id, string userId)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        if (document == null)
        {
            return false;
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }
}