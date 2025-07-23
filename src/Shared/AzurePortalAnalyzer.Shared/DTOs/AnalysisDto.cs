namespace AzurePortalAnalyzer.Shared.DTOs;

/// <summary>
/// DTO for creating a new configuration analysis
/// </summary>
public class CreateAnalysisRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConfigurationData { get; set; } = string.Empty;
}

/// <summary>
/// DTO for analysis response
/// </summary>
public class AnalysisResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AnalysisResultDto> Results { get; set; } = new();
}

/// <summary>
/// DTO for analysis result
/// </summary>
public class AnalysisResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Standard API response wrapper
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}