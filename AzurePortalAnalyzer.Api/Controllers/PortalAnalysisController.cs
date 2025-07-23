using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AzurePortalAnalyzer.Api.Models;
using AzurePortalAnalyzer.Api.Services;

namespace AzurePortalAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortalAnalysisController : ControllerBase
{
    private readonly IPortalAnalysisService _portalAnalysisService;
    private readonly ILogger<PortalAnalysisController> _logger;

    public PortalAnalysisController(IPortalAnalysisService portalAnalysisService, ILogger<PortalAnalysisController> logger)
    {
        _portalAnalysisService = portalAnalysisService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found");
    }

    [HttpPost]
    public async Task<ActionResult<PortalAnalysis>> CreateAnalysis([FromBody] CreatePortalAnalysisRequest request)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _portalAnalysisService.CreateAnalysisAsync(request, userId);
            return CreatedAtAction(nameof(GetAnalysis), new { id = analysis.Id }, analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portal analysis");
            return StatusCode(500, "An error occurred while creating the analysis");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PortalAnalysis>> GetAnalysis(int id)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _portalAnalysisService.GetAnalysisAsync(id, userId);
            
            if (analysis == null)
            {
                return NotFound();
            }

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portal analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while retrieving the analysis");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<PortalAnalysis>>> GetUserAnalyses()
    {
        try
        {
            var userId = GetUserId();
            var analyses = await _portalAnalysisService.GetUserAnalysesAsync(userId);
            return Ok(analyses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user analyses");
            return StatusCode(500, "An error occurred while retrieving analyses");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PortalAnalysis>> UpdateAnalysis(int id, [FromBody] CreatePortalAnalysisRequest request)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _portalAnalysisService.UpdateAnalysisAsync(id, request, userId);
            return Ok(analysis);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portal analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while updating the analysis");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAnalysis(int id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _portalAnalysisService.DeleteAnalysisAsync(id, userId);
            
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portal analysis {AnalysisId}", id);
            return StatusCode(500, "An error occurred while deleting the analysis");
        }
    }
}