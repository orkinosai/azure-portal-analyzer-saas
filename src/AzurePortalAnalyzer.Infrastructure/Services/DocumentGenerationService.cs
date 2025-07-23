using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Markdig;
using AzurePortalAnalyzer.Core.Services;
using System.Text;

namespace AzurePortalAnalyzer.Infrastructure.Services;

public class DocumentGenerationService : IDocumentGenerationService
{
    public DocumentGenerationService()
    {
        // Configure QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfReportAsync(object reportData, string templateName)
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
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Azure Portal Analysis Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Report Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                            
                            x.Item().Text("Executive Summary")
                                .FontSize(16).SemiBold();
                            
                            x.Item().Text("This report provides a comprehensive analysis of your Azure Portal configuration and usage patterns.");

                            x.Item().Text("Key Findings")
                                .FontSize(16).SemiBold();

                            x.Item().Text("• Resource utilization analysis");
                            x.Item().Text("• Security recommendations");
                            x.Item().Text("• Cost optimization opportunities");
                            x.Item().Text("• Compliance status overview");

                            x.Item().Text("Recommendations")
                                .FontSize(16).SemiBold();

                            x.Item().Text("Based on the analysis, we recommend the following actions:");
                            x.Item().Text("• Enable monitoring for critical resources");
                            x.Item().Text("• Review and update access policies");
                            x.Item().Text("• Implement cost management alerts");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<string> GenerateMarkdownReportAsync(object reportData, string templateName)
    {
        return await Task.Run(() =>
        {
            var markdown = new StringBuilder();
            
            markdown.AppendLine("# Azure Portal Analysis Report");
            markdown.AppendLine();
            markdown.AppendLine($"**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
            markdown.AppendLine();
            
            markdown.AppendLine("## Executive Summary");
            markdown.AppendLine();
            markdown.AppendLine("This report provides a comprehensive analysis of your Azure Portal configuration and usage patterns.");
            markdown.AppendLine();
            
            markdown.AppendLine("## Key Findings");
            markdown.AppendLine();
            markdown.AppendLine("- Resource utilization analysis");
            markdown.AppendLine("- Security recommendations");
            markdown.AppendLine("- Cost optimization opportunities");
            markdown.AppendLine("- Compliance status overview");
            markdown.AppendLine();
            
            markdown.AppendLine("## Recommendations");
            markdown.AppendLine();
            markdown.AppendLine("Based on the analysis, we recommend the following actions:");
            markdown.AppendLine();
            markdown.AppendLine("1. **Enable monitoring** for critical resources");
            markdown.AppendLine("2. **Review and update** access policies");
            markdown.AppendLine("3. **Implement cost management** alerts");
            markdown.AppendLine("4. **Regular security audits** of your environment");
            markdown.AppendLine();
            
            markdown.AppendLine("## Detailed Analysis");
            markdown.AppendLine();
            markdown.AppendLine("### Resource Inventory");
            markdown.AppendLine("- Virtual Machines: Analysis pending");
            markdown.AppendLine("- Storage Accounts: Analysis pending");
            markdown.AppendLine("- Databases: Analysis pending");
            markdown.AppendLine();
            
            markdown.AppendLine("### Security Assessment");
            markdown.AppendLine("- Identity and Access Management: Under review");
            markdown.AppendLine("- Network Security: Under review");
            markdown.AppendLine("- Data Protection: Under review");
            markdown.AppendLine();

            return markdown.ToString();
        });
    }
}