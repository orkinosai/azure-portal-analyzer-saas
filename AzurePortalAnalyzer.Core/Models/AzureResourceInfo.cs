namespace AzurePortalAnalyzer.Core.Models;

public class AzureResourceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ResourceGroup { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public Dictionary<string, string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}