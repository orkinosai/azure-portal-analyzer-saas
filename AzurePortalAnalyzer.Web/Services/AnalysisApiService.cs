using AzurePortalAnalyzer.Web.Models;
using System.Text.Json;
using System.Text;

namespace AzurePortalAnalyzer.Web.Services;

public class AnalysisApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnalysisApiService> _logger;

    public AnalysisApiService(HttpClient httpClient, ILogger<AnalysisApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AnalysisRequestResponse?> CreateAnalysisRequestAsync(AnalysisRequestModel request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/analysis", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AnalysisRequestResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            else
            {
                _logger.LogError("Failed to create analysis request. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating analysis request");
            return null;
        }
    }

    public async Task<AnalysisRequestResponse?> GetAnalysisRequestAsync(Guid requestId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/analysis/{requestId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AnalysisRequestResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                _logger.LogError("Failed to get analysis request. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting analysis request {RequestId}", requestId);
            return null;
        }
    }

    public async Task<bool> ReprocessAnalysisRequestAsync(Guid requestId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/analysis/{requestId}/reprocess", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while reprocessing analysis request {RequestId}", requestId);
            return false;
        }
    }
}