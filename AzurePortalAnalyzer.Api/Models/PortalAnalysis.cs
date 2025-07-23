using System.ComponentModel.DataAnnotations;

namespace AzurePortalAnalyzer.Api.Models;

public class PortalAnalysis
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public List<PortalComponent> Components { get; set; } = [];
    
    public List<RequirementDocument> Documents { get; set; } = [];
}

public class PortalComponent
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty; // e.g., "Button", "Form", "Navigation", "Dashboard"
    
    public string? Description { get; set; }
    
    public Dictionary<string, object> Properties { get; set; } = [];
    
    public List<string> Requirements { get; set; } = [];
    
    public int PortalAnalysisId { get; set; }
    
    public PortalAnalysis PortalAnalysis { get; set; } = null!;
}

public class RequirementDocument
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Required]
    public string Format { get; set; } = string.Empty; // "PDF", "Markdown", "HTML"
    
    public string? BlobPath { get; set; }
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    public int PortalAnalysisId { get; set; }
    
    public PortalAnalysis PortalAnalysis { get; set; } = null!;
}