using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Infrastructure.Services;

namespace AzurePortalAnalyzer.Tests;

public class DocumentGenerationServiceTests
{
    [Fact]
    public async Task GeneratePdfReportAsync_Should_Return_ValidPdfBytes()
    {
        // Arrange
        var service = new DocumentGenerationService();
        var reportData = new { Title = "Test Report" };

        // Act
        var result = await service.GeneratePdfReportAsync(reportData, "test-template");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // Check if it starts with PDF header
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        Assert.Equal(pdfHeader, result.Take(4).ToArray());
    }

    [Fact]
    public async Task GenerateMarkdownReportAsync_Should_Return_ValidMarkdown()
    {
        // Arrange
        var service = new DocumentGenerationService();
        var reportData = new { Title = "Test Report" };

        // Act
        var result = await service.GenerateMarkdownReportAsync(reportData, "test-template");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Azure Portal Analysis Report", result);
        Assert.Contains("## Executive Summary", result);
        Assert.Contains("## Key Findings", result);
        Assert.Contains("## Recommendations", result);
    }
}

public class DocumentModelTests
{
    [Fact]
    public void Document_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var document = new Document();

        // Assert
        Assert.Equal(0, document.Id);
        Assert.Equal(string.Empty, document.Title);
        Assert.Equal(string.Empty, document.FileName);
        Assert.Equal(string.Empty, document.BlobName);
        Assert.Equal(string.Empty, document.ContentType);
        Assert.Equal(0, document.Size);
        Assert.Equal(string.Empty, document.UserId);
        Assert.Equal(DocumentStatus.Draft, document.Status);
        Assert.Equal(DocumentType.PortalAnalysisReport, document.Type);
    }

    [Fact]
    public void Document_Should_Allow_Property_Assignment()
    {
        // Arrange
        var document = new Document();
        var testDate = DateTime.UtcNow;

        // Act
        document.Id = 1;
        document.Title = "Test Document";
        document.FileName = "test.pdf";
        document.BlobName = "blob123";
        document.ContentType = "application/pdf";
        document.Size = 1024;
        document.UserId = "user123";
        document.CreatedAt = testDate;
        document.UpdatedAt = testDate;
        document.Status = DocumentStatus.Generated;
        document.Type = DocumentType.ComplianceReport;

        // Assert
        Assert.Equal(1, document.Id);
        Assert.Equal("Test Document", document.Title);
        Assert.Equal("test.pdf", document.FileName);
        Assert.Equal("blob123", document.BlobName);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.Equal(1024, document.Size);
        Assert.Equal("user123", document.UserId);
        Assert.Equal(testDate, document.CreatedAt);
        Assert.Equal(testDate, document.UpdatedAt);
        Assert.Equal(DocumentStatus.Generated, document.Status);
        Assert.Equal(DocumentType.ComplianceReport, document.Type);
    }
}