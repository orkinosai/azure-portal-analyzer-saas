using Azure;
using Azure.Communication.Email;
using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzurePortalAnalyzer.Core.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<EmailService> _logger;
    private readonly string _senderAddress;

    public EmailService(EmailClient emailClient, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _logger = logger;
        _senderAddress = configuration["Azure:Communication:SenderAddress"] ?? "noreply@azureanalyzer.com";
    }

    public async Task SendDocumentAsync(string recipientEmail, string documentUrl, string fileName)
    {
        try
        {
            var emailMessage = new EmailMessage(
                senderAddress: _senderAddress,
                recipientAddress: recipientEmail,
                content: new EmailContent("Azure Portal Analysis Complete")
                {
                    PlainText = $"Your Azure Portal analysis document is ready. You can download it from: {documentUrl}",
                    Html = $@"
                        <h2>Azure Portal Analysis Complete</h2>
                        <p>Your Azure Portal analysis has been completed successfully.</p>
                        <p><a href=""{documentUrl}"" target=""_blank"">Download your analysis document ({fileName})</a></p>
                        <p>Best regards,<br/>Azure Portal Analyzer Team</p>
                    "
                });

            var operation = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            _logger.LogInformation("Email sent successfully to {Email} with document {FileName}", recipientEmail, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", recipientEmail);
            throw;
        }
    }

    public async Task SendAnalysisCompleteAsync(string recipientEmail, AnalysisResult result, string documentUrl)
    {
        try
        {
            var emailMessage = new EmailMessage(
                senderAddress: _senderAddress,
                recipientAddress: recipientEmail,
                content: new EmailContent("Azure Portal Analysis Complete - Detailed Results")
                {
                    PlainText = GeneratePlainTextSummary(result, documentUrl),
                    Html = GenerateHtmlSummary(result, documentUrl)
                });

            var operation = await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            _logger.LogInformation("Analysis complete email sent successfully to {Email}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending analysis complete email to {Email}", recipientEmail);
            throw;
        }
    }

    private string GeneratePlainTextSummary(AnalysisResult result, string documentUrl)
    {
        return $@"
Azure Portal Analysis Complete

Analysis Summary:
- Total Resources: {result.Summary.TotalResources}
- Resource Groups: {result.Summary.ResourceGroupCount}
- Estimated Monthly Cost: ${result.Summary.EstimatedMonthlyCost:F2}
- Recommendations: {result.Recommendations.Count}

Download your detailed analysis document: {documentUrl}

Best regards,
Azure Portal Analyzer Team
        ";
    }

    private string GenerateHtmlSummary(AnalysisResult result, string documentUrl)
    {
        var resourceTypesHtml = string.Join("", result.Summary.ResourceTypeCount.Select(rt => 
            $"<li>{rt.Key}: {rt.Value}</li>"));

        var topRecommendationsHtml = string.Join("", result.Recommendations.Take(3).Select(r => 
            $"<li><strong>{r.Title}</strong> - Priority: {r.Priority}</li>"));

        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Azure Portal Analysis Complete</h2>
                
                <h3>Analysis Summary</h3>
                <ul>
                    <li><strong>Total Resources:</strong> {result.Summary.TotalResources}</li>
                    <li><strong>Resource Groups:</strong> {result.Summary.ResourceGroupCount}</li>
                    <li><strong>Estimated Monthly Cost:</strong> ${result.Summary.EstimatedMonthlyCost:F2}</li>
                    <li><strong>Total Recommendations:</strong> {result.Recommendations.Count}</li>
                </ul>

                <h3>Resource Types</h3>
                <ul>{resourceTypesHtml}</ul>

                <h3>Top Recommendations</h3>
                <ul>{topRecommendationsHtml}</ul>

                <p><a href='{documentUrl}' target='_blank' style='background-color: #0078d4; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Download Detailed Analysis Document</a></p>

                <p>Best regards,<br/>Azure Portal Analyzer Team</p>
            </body>
            </html>
        ";
    }
}