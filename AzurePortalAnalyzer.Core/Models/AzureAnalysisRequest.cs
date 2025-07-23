using System.ComponentModel.DataAnnotations;

namespace AzurePortalAnalyzer.Core.Models;

public class AzureAnalysisRequest
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;
    
    [Required]
    public string SubscriptionId { get; set; } = string.Empty;
    
    public string? ResourceGroup { get; set; }
    
    public List<string> ResourceTypes { get; set; } = new();
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    
    public string? DocumentUrl { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}

public enum AnalysisStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    EmailSent
}