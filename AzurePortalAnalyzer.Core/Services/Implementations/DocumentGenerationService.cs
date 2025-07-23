using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using Markdig;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AzurePortalAnalyzer.Core.Services.Implementations;

public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ILogger<DocumentGenerationService> _logger;

    public DocumentGenerationService(ILogger<DocumentGenerationService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePdfDocumentAsync(AnalysisResult analysisResult)
    {
        try
        {
            _logger.LogInformation("Generating PDF document for request {RequestId}", analysisResult.RequestId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Azure Portal Analysis Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            // Summary Section
                            x.Item().Text("Analysis Summary").SemiBold().FontSize(16);
                            x.Item().PaddingLeft(20).Column(summary =>
                            {
                                summary.Item().Text($"Generated: {analysisResult.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
                                summary.Item().Text($"Total Resources: {analysisResult.Summary.TotalResources}");
                                summary.Item().Text($"Resource Groups: {analysisResult.Summary.ResourceGroupCount}");
                                summary.Item().Text($"Estimated Monthly Cost: ${analysisResult.Summary.EstimatedMonthlyCost:F2}");
                            });

                            // Resource Types Section
                            x.Item().Text("Resource Types Distribution").SemiBold().FontSize(16);
                            x.Item().PaddingLeft(20).Column(resourceTypes =>
                            {
                                foreach (var resourceType in analysisResult.Summary.ResourceTypeCount)
                                {
                                    resourceTypes.Item().Text($"{resourceType.Key}: {resourceType.Value}");
                                }
                            });

                            // Recommendations Section
                            x.Item().Text("Recommendations").SemiBold().FontSize(16);
                            x.Item().PaddingLeft(20).Column(recommendations =>
                            {
                                foreach (var recommendation in analysisResult.Recommendations.Take(10))
                                {
                                    recommendations.Item().PaddingBottom(10).Column(rec =>
                                    {
                                        rec.Item().Text(recommendation.Title).SemiBold().FontColor(Colors.Blue.Darken2);
                                        rec.Item().Text($"Priority: {recommendation.Priority}");
                                        rec.Item().Text(recommendation.Description);
                                        if (recommendation.PotentialSavings > 0)
                                        {
                                            rec.Item().Text($"Potential Savings: ${recommendation.PotentialSavings:F2}/month")
                                                .FontColor(Colors.Green.Medium);
                                        }
                                    });
                                }
                            });

                            // Resources Details Section (Top 20)
                            x.Item().Text("Resource Details (Top 20)").SemiBold().FontSize(16);
                            x.Item().PaddingLeft(20).Column(resources =>
                            {
                                foreach (var resource in analysisResult.Resources.Take(20))
                                {
                                    resources.Item().PaddingBottom(8).Column(res =>
                                    {
                                        res.Item().Text(resource.Name).SemiBold();
                                        res.Item().Text($"Type: {resource.Type}");
                                        res.Item().Text($"Location: {resource.Location}");
                                        res.Item().Text($"Resource Group: {resource.ResourceGroup}");
                                        res.Item().Text($"Status: {resource.Status}");
                                    });
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();
            _logger.LogInformation("PDF document generated successfully for request {RequestId}", analysisResult.RequestId);
            return Task.FromResult(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF document for request {RequestId}", analysisResult.RequestId);
            throw;
        }
    }

    public Task<string> GenerateMarkdownDocumentAsync(AnalysisResult analysisResult)
    {
        try
        {
            _logger.LogInformation("Generating Markdown document for request {RequestId}", analysisResult.RequestId);

            var markdown = $@"# Azure Portal Analysis Report

## Analysis Summary

- **Generated**: {analysisResult.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC
- **Total Resources**: {analysisResult.Summary.TotalResources}
- **Resource Groups**: {analysisResult.Summary.ResourceGroupCount}
- **Estimated Monthly Cost**: ${analysisResult.Summary.EstimatedMonthlyCost:F2}

## Resource Types Distribution

{string.Join("\n", analysisResult.Summary.ResourceTypeCount.Select(rt => $"- **{rt.Key}**: {rt.Value}"))}

## Recommendations

{string.Join("\n\n", analysisResult.Recommendations.Select(r => $@"### {r.Title}

**Priority**: {r.Priority}

{r.Description}

{(r.PotentialSavings > 0 ? $"**Potential Savings**: ${r.PotentialSavings:F2}/month" : "")}
"))}

## Resource Details

{string.Join("\n\n", analysisResult.Resources.Take(50).Select(r => $@"### {r.Name}

- **Type**: {r.Type}
- **Location**: {r.Location}
- **Resource Group**: {r.ResourceGroup}
- **Status**: {r.Status}
{(r.Tags.Any() ? $"- **Tags**: {string.Join(", ", r.Tags.Select(t => $"{t.Key}={t.Value}"))}" : "")}
"))}

---
*Report generated by Azure Portal Analyzer*
";

            _logger.LogInformation("Markdown document generated successfully for request {RequestId}", analysisResult.RequestId);
            return Task.FromResult(markdown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Markdown document for request {RequestId}", analysisResult.RequestId);
            throw;
        }
    }
}