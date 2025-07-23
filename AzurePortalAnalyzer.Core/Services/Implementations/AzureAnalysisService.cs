using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using Microsoft.Extensions.Logging;

namespace AzurePortalAnalyzer.Core.Services.Implementations;

public class AzureAnalysisService : IAzureAnalysisService
{
    private readonly ILogger<AzureAnalysisService> _logger;
    private readonly List<AzureAnalysisRequest> _requests = new(); // In-memory storage for demo

    public AzureAnalysisService(ILogger<AzureAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<AnalysisResult> AnalyzeAzureResourcesAsync(AzureAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Starting Azure resource analysis for request {RequestId}", request.Id);

            // Simulate Azure resource discovery and analysis
            await Task.Delay(2000); // Simulate processing time

            var resources = GenerateSampleResources(request);
            var summary = GenerateSummary(resources);
            var recommendations = GenerateRecommendations(resources);

            var result = new AnalysisResult
            {
                RequestId = request.Id,
                Resources = resources,
                Summary = summary,
                Recommendations = recommendations,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Azure resource analysis completed for request {RequestId}. Found {ResourceCount} resources", 
                request.Id, resources.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing Azure resources for request {RequestId}", request.Id);
            throw;
        }
    }

    public async Task<AzureAnalysisRequest> GetRequestAsync(Guid requestId)
    {
        var request = _requests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            throw new InvalidOperationException($"Request {requestId} not found");
        }
        return await Task.FromResult(request);
    }

    public async Task<AzureAnalysisRequest> CreateRequestAsync(AzureAnalysisRequest request)
    {
        request.Id = Guid.NewGuid();
        request.RequestedAt = DateTime.UtcNow;
        request.Status = AnalysisStatus.Pending;
        
        _requests.Add(request);
        
        _logger.LogInformation("Created analysis request {RequestId} for user {UserEmail}", request.Id, request.UserEmail);
        return await Task.FromResult(request);
    }

    public async Task UpdateRequestStatusAsync(Guid requestId, AnalysisStatus status, string? errorMessage = null)
    {
        var request = _requests.FirstOrDefault(r => r.Id == requestId);
        if (request != null)
        {
            request.Status = status;
            request.ErrorMessage = errorMessage;
            
            if (status == AnalysisStatus.Completed || status == AnalysisStatus.Failed)
            {
                request.CompletedAt = DateTime.UtcNow;
            }
            
            _logger.LogInformation("Updated request {RequestId} status to {Status}", requestId, status);
        }
        
        await Task.CompletedTask;
    }

    private List<AzureResourceInfo> GenerateSampleResources(AzureAnalysisRequest request)
    {
        var resources = new List<AzureResourceInfo>();
        var random = new Random();
        
        var resourceTypes = new[]
        {
            "Microsoft.Compute/virtualMachines",
            "Microsoft.Storage/storageAccounts",
            "Microsoft.Web/sites",
            "Microsoft.Sql/servers",
            "Microsoft.Network/virtualNetworks",
            "Microsoft.KeyVault/vaults",
            "Microsoft.Insights/components"
        };

        var locations = new[] { "East US", "West US 2", "Central US", "North Europe", "West Europe" };
        var statuses = new[] { "Running", "Stopped", "Deallocated", "Available" };

        for (int i = 0; i < random.Next(10, 25); i++)
        {
            var resourceType = resourceTypes[random.Next(resourceTypes.Length)];
            var resourceName = $"{resourceType.Split('/')[1].ToLower()}-{random.Next(1000, 9999)}";
            
            resources.Add(new AzureResourceInfo
            {
                Id = $"/subscriptions/{request.SubscriptionId}/resourceGroups/{request.ResourceGroup ?? "default-rg"}/providers/{resourceType}/{resourceName}",
                Name = resourceName,
                Type = resourceType,
                Location = locations[random.Next(locations.Length)],
                ResourceGroup = request.ResourceGroup ?? "default-rg",
                Status = statuses[random.Next(statuses.Length)],
                Tags = new Dictionary<string, string>
                {
                    ["Environment"] = random.Next(2) == 0 ? "Production" : "Development",
                    ["CostCenter"] = $"CC-{random.Next(1000, 9999)}"
                },
                Properties = new Dictionary<string, object>
                {
                    ["CreatedTime"] = DateTime.UtcNow.AddDays(-random.Next(1, 365)),
                    ["Size"] = resourceType.Contains("Compute") ? $"Standard_B{random.Next(1, 4)}s" : "N/A"
                }
            });
        }

        return resources;
    }

    private AnalysisSummary GenerateSummary(List<AzureResourceInfo> resources)
    {
        var resourceTypeCount = resources.GroupBy(r => r.Type)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var locationCount = resources.GroupBy(r => r.Location)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var resourceGroupCount = resources.Select(r => r.ResourceGroup)
            .Distinct().Count();

        return new AnalysisSummary
        {
            TotalResources = resources.Count,
            ResourceTypeCount = resourceTypeCount,
            LocationCount = locationCount,
            ResourceGroupCount = resourceGroupCount,
            EstimatedMonthlyCost = new Random().Next(100, 2000) // Mock cost calculation
        };
    }

    private List<Recommendation> GenerateRecommendations(List<AzureResourceInfo> resources)
    {
        var recommendations = new List<Recommendation>();
        var random = new Random();

        // Sample recommendations based on resources
        if (resources.Any(r => r.Type.Contains("virtualMachines") && r.Status == "Stopped"))
        {
            recommendations.Add(new Recommendation
            {
                Type = "Cost Optimization",
                Title = "Deallocate Stopped Virtual Machines",
                Description = "You have stopped VMs that are still incurring charges. Consider deallocating them to save costs.",
                Priority = "High",
                PotentialSavings = random.Next(50, 200)
            });
        }

        if (resources.Any(r => r.Tags.GetValueOrDefault("Environment") == "Development"))
        {
            recommendations.Add(new Recommendation
            {
                Type = "Resource Management",
                Title = "Review Development Resources",
                Description = "Consider using lower-cost tiers for development resources or implement auto-shutdown policies.",
                Priority = "Medium",
                PotentialSavings = random.Next(20, 100)
            });
        }

        recommendations.Add(new Recommendation
        {
            Type = "Security",
            Title = "Enable Azure Security Center",
            Description = "Implement Azure Security Center recommendations to improve your security posture.",
            Priority = "High",
            PotentialSavings = 0
        });

        recommendations.Add(new Recommendation
        {
            Type = "Governance",
            Title = "Implement Resource Tagging Strategy",
            Description = "Ensure all resources have proper tags for cost tracking and resource management.",
            Priority = "Medium",
            PotentialSavings = 0
        });

        return recommendations;
    }
}