using AzurePortalAnalyzer.API.DTOs;
using AzurePortalAnalyzer.Core.Models;
using AzurePortalAnalyzer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzurePortalAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IAzureAnalysisService _azureAnalysisService;
    private readonly IAnalysisOrchestrationService _orchestrationService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IAzureAnalysisService azureAnalysisService,
        IAnalysisOrchestrationService orchestrationService,
        ILogger<AnalysisController> logger)
    {
        _azureAnalysisService = azureAnalysisService;
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AnalysisRequestResponseDto>> CreateAnalysisRequest([FromBody] CreateAnalysisRequestDto requestDto)
    {
        try
        {
            var request = new AzureAnalysisRequest
            {
                UserEmail = requestDto.UserEmail,
                SubscriptionId = requestDto.SubscriptionId,
                ResourceGroup = requestDto.ResourceGroup,
                ResourceTypes = requestDto.ResourceTypes
            };

            var createdRequest = await _azureAnalysisService.CreateRequestAsync(request);
            
            // Start the analysis process in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _orchestrationService.ProcessAnalysisRequestAsync(createdRequest.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing analysis request {RequestId}", createdRequest.Id);
                }
            });

            var responseDto = MapToResponseDto(createdRequest);
            return CreatedAtAction(nameof(GetAnalysisRequest), new { id = createdRequest.Id }, responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating analysis request");
            return StatusCode(500, "An error occurred while creating the analysis request");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnalysisRequestResponseDto>> GetAnalysisRequest(Guid id)
    {
        try
        {
            var request = await _azureAnalysisService.GetRequestAsync(id);
            var responseDto = MapToResponseDto(request);
            return Ok(responseDto);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Analysis request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis request {RequestId}", id);
            return StatusCode(500, "An error occurred while retrieving the analysis request");
        }
    }

    [HttpPost("{id}/reprocess")]
    public async Task<ActionResult> ReprocessAnalysisRequest(Guid id)
    {
        try
        {
            var request = await _azureAnalysisService.GetRequestAsync(id);
            
            // Start the analysis process in the background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _orchestrationService.ProcessAnalysisRequestAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reprocessing analysis request {RequestId}", id);
                }
            });

            return Accepted();
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Analysis request {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reprocessing analysis request {RequestId}", id);
            return StatusCode(500, "An error occurred while reprocessing the analysis request");
        }
    }

    private static AnalysisRequestResponseDto MapToResponseDto(AzureAnalysisRequest request)
    {
        return new AnalysisRequestResponseDto
        {
            Id = request.Id,
            UserEmail = request.UserEmail,
            SubscriptionId = request.SubscriptionId,
            ResourceGroup = request.ResourceGroup,
            ResourceTypes = request.ResourceTypes,
            RequestedAt = request.RequestedAt,
            Status = request.Status.ToString(),
            DocumentUrl = request.DocumentUrl,
            CompletedAt = request.CompletedAt,
            ErrorMessage = request.ErrorMessage
        };
    }
}