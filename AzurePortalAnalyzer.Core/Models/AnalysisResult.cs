namespace AzurePortalAnalyzer.Core.Models;

public class AnalysisResult
{
    public Guid RequestId { get; set; }
    public List<AzureResourceInfo> Resources { get; set; } = new();
    public AnalysisSummary Summary { get; set; } = new();
    public List<Recommendation> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class AnalysisSummary
{
    public int TotalResources { get; set; }
    public Dictionary<string, int> ResourceTypeCount { get; set; } = new();
    public Dictionary<string, int> LocationCount { get; set; } = new();
    public int ResourceGroupCount { get; set; }
    public decimal EstimatedMonthlyCost { get; set; }
}

public class Recommendation
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
}