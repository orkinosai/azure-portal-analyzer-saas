namespace AzurePortalAnalyzer.Api.Models;

public class CreatePortalAnalysisRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreatePortalComponentRequest> Components { get; set; } = [];
}

public class CreatePortalComponentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object> Properties { get; set; } = [];
    public List<string> Requirements { get; set; } = [];
}

public class GenerateDocumentRequest
{
    public int PortalAnalysisId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Format { get; set; } = "PDF"; // PDF, Markdown, HTML
}

public class DocumentGenerationResult
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public DateTime GeneratedAt { get; set; }
}