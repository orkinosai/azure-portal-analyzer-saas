using Xunit;
using AzurePortalAnalyzer.Api.Models;
using AzurePortalAnalyzer.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AzurePortalAnalyzer.Tests;

public class DocumentGenerationServiceTests
{
    [Fact]
    public async Task GenerateMarkdownAsync_ShouldReturnValidMarkdown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DocumentGenerationService>>();
        var service = new DocumentGenerationService(mockLogger.Object);
        
        var analysis = new PortalAnalysis
        {
            Id = 1,
            Name = "Test Analysis",
            Description = "Test Description",
            UserId = "test-user",
            CreatedAt = DateTime.UtcNow,
            Components = 
            [
                new PortalComponent
                {
                    Id = 1,
                    Name = "Login Form",
                    Type = "Form",
                    Description = "User login form",
                    Requirements = ["Username field required", "Password field required", "Submit button"]
                }
            ]
        };

        // Act
        var result = await service.GenerateMarkdownAsync(analysis, "Test Requirements", "Test document");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Test Requirements", result);
        Assert.Contains("Test Analysis", result);
        Assert.Contains("Login Form", result);
        Assert.Contains("Username field required", result);
    }

    [Fact]
    public async Task GeneratePdfAsync_ShouldReturnPdfBytes()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DocumentGenerationService>>();
        var service = new DocumentGenerationService(mockLogger.Object);
        
        var analysis = new PortalAnalysis
        {
            Id = 1,
            Name = "Test Analysis",
            Description = "Test Description",
            UserId = "test-user",
            CreatedAt = DateTime.UtcNow,
            Components = 
            [
                new PortalComponent
                {
                    Id = 1,
                    Name = "Dashboard",
                    Type = "View",
                    Description = "Main dashboard view",
                    Requirements = ["Display user metrics", "Show charts"]
                }
            ]
        };

        // Act
        var result = await service.GeneratePdfAsync(analysis, "Test PDF Requirements");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        // PDF files start with %PDF
        Assert.Equal(0x25, result[0]); // %
        Assert.Equal(0x50, result[1]); // P
        Assert.Equal(0x44, result[2]); // D
        Assert.Equal(0x46, result[3]); // F
    }
}