using System.ComponentModel.DataAnnotations;

namespace AzurePortalAnalyzer.Web.Models;

public class AnalysisRequestModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string UserEmail { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Azure Subscription ID")]
    public string SubscriptionId { get; set; } = string.Empty;
    
    [Display(Name = "Resource Group (Optional)")]
    public string? ResourceGroup { get; set; }
    
    [Display(Name = "Resource Types to Analyze")]
    public List<string> ResourceTypes { get; set; } = new();
}

public class AnalysisRequestResponse
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