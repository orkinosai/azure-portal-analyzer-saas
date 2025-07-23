using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Markdig;
using AzurePortalAnalyzer.Api.Models;

namespace AzurePortalAnalyzer.Api.Services;

public interface IDocumentGenerationService
{
    Task<byte[]> GeneratePdfAsync(PortalAnalysis analysis, string title, string? description = null);
    Task<string> GenerateMarkdownAsync(PortalAnalysis analysis, string title, string? description = null);
    Task<string> GenerateHtmlAsync(PortalAnalysis analysis, string title, string? description = null);
}

public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ILogger<DocumentGenerationService> _logger;

    public DocumentGenerationService(ILogger<DocumentGenerationService> logger)
    {
        _logger = logger;
        
        // Set QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfAsync(PortalAnalysis analysis, string title, string? description = null)
    {
        try
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .Text(title)
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                // Analysis Overview
                                x.Item().Text("Portal Analysis Overview").SemiBold().FontSize(16);
                                x.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                x.Item().Text($"Analysis Name: {analysis.Name}").FontSize(12);
                                x.Item().Text($"Created: {analysis.CreatedAt:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                                
                                if (!string.IsNullOrEmpty(description))
                                {
                                    x.Item().PaddingTop(0.5f, Unit.Centimetre).Text(description).FontSize(11);
                                }
                                
                                if (!string.IsNullOrEmpty(analysis.Description))
                                {
                                    x.Item().PaddingTop(0.5f, Unit.Centimetre).Text(analysis.Description).FontSize(11);
                                }

                                x.Item().PaddingVertical(1, Unit.Centimetre);

                                // Components Section
                                x.Item().Text("Portal Components").SemiBold().FontSize(16);
                                x.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                foreach (var component in analysis.Components)
                                {
                                    x.Item().PaddingBottom(0.5f, Unit.Centimetre).Column(col =>
                                    {
                                        col.Item().Text($"• {component.Name} ({component.Type})").SemiBold().FontSize(12);
                                        
                                        if (!string.IsNullOrEmpty(component.Description))
                                        {
                                            col.Item().PaddingLeft(0.5f, Unit.Centimetre).Text(component.Description).FontSize(10);
                                        }

                                        if (component.Requirements.Any())
                                        {
                                            col.Item().PaddingLeft(0.5f, Unit.Centimetre).PaddingTop(0.2f, Unit.Centimetre).Text("Requirements:").FontSize(10).SemiBold();
                                            foreach (var requirement in component.Requirements)
                                            {
                                                col.Item().PaddingLeft(1, Unit.Centimetre).Text($"- {requirement}").FontSize(9);
                                            }
                                        }
                                    });
                                }

                                // Requirements Summary
                                var allRequirements = analysis.Components.SelectMany(c => c.Requirements).Distinct().ToList();
                                if (allRequirements.Any())
                                {
                                    x.Item().PaddingTop(1, Unit.Centimetre);
                                    x.Item().Text("All Requirements Summary").SemiBold().FontSize(16);
                                    x.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                    foreach (var requirement in allRequirements)
                                    {
                                        x.Item().Text($"• {requirement}").FontSize(11);
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generated on ");
                                x.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").SemiBold();
                            });
                    });
                });

                return document.GeneratePdf();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for analysis: {AnalysisId}", analysis.Id);
            throw;
        }
    }

    public async Task<string> GenerateMarkdownAsync(PortalAnalysis analysis, string title, string? description = null)
    {
        try
        {
            return await Task.Run(() =>
            {
                var markdown = new System.Text.StringBuilder();

                markdown.AppendLine($"# {title}");
                markdown.AppendLine();

                if (!string.IsNullOrEmpty(description))
                {
                    markdown.AppendLine(description);
                    markdown.AppendLine();
                }

                markdown.AppendLine("## Portal Analysis Overview");
                markdown.AppendLine();
                markdown.AppendLine($"**Analysis Name:** {analysis.Name}");
                markdown.AppendLine($"**Created:** {analysis.CreatedAt:yyyy-MM-dd HH:mm} UTC");
                markdown.AppendLine();

                if (!string.IsNullOrEmpty(analysis.Description))
                {
                    markdown.AppendLine(analysis.Description);
                    markdown.AppendLine();
                }

                markdown.AppendLine("## Portal Components");
                markdown.AppendLine();

                foreach (var component in analysis.Components)
                {
                    markdown.AppendLine($"### {component.Name} ({component.Type})");
                    markdown.AppendLine();

                    if (!string.IsNullOrEmpty(component.Description))
                    {
                        markdown.AppendLine(component.Description);
                        markdown.AppendLine();
                    }

                    if (component.Requirements.Any())
                    {
                        markdown.AppendLine("**Requirements:**");
                        foreach (var requirement in component.Requirements)
                        {
                            markdown.AppendLine($"- {requirement}");
                        }
                        markdown.AppendLine();
                    }

                    if (component.Properties.Any())
                    {
                        markdown.AppendLine("**Properties:**");
                        foreach (var prop in component.Properties)
                        {
                            markdown.AppendLine($"- **{prop.Key}:** {prop.Value}");
                        }
                        markdown.AppendLine();
                    }
                }

                var allRequirements = analysis.Components.SelectMany(c => c.Requirements).Distinct().ToList();
                if (allRequirements.Any())
                {
                    markdown.AppendLine("## All Requirements Summary");
                    markdown.AppendLine();
                    foreach (var requirement in allRequirements)
                    {
                        markdown.AppendLine($"- {requirement}");
                    }
                    markdown.AppendLine();
                }

                markdown.AppendLine("---");
                markdown.AppendLine($"*Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC*");

                return markdown.ToString();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Markdown for analysis: {AnalysisId}", analysis.Id);
            throw;
        }
    }

    public async Task<string> GenerateHtmlAsync(PortalAnalysis analysis, string title, string? description = null)
    {
        try
        {
            var markdown = await GenerateMarkdownAsync(analysis, title, description);
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(markdown, pipeline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HTML for analysis: {AnalysisId}", analysis.Id);
            throw;
        }
    }
}