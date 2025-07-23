namespace AzurePortalAnalyzer.Shared.Models;

/// <summary>
/// Represents an Azure Portal configuration analysis
/// </summary>
public class ConfigurationAnalysis
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public List<AnalysisResult> Results { get; set; } = new();
}

/// <summary>
/// Represents a single analysis result
/// </summary>
public class AnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
    public string Recommendation { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents an Azure resource configuration
/// </summary>
public class AzureResourceConfig
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}
