using System.ComponentModel.DataAnnotations;

namespace AzurePortalAnalyzer.API.DTOs;

public class CreateAnalysisRequestDto
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;
    
    [Required]
    public string SubscriptionId { get; set; } = string.Empty;
    
    public string? ResourceGroup { get; set; }
    
    public List<string> ResourceTypes { get; set; } = new();
}

public class AnalysisRequestResponseDto
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string? ResourceGroup { get; set; }
    public List<string> ResourceTypes { get; set; } = new();
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}