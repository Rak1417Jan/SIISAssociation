using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;

namespace MVEA.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AssemblyController : ControllerBase
{
    private readonly IAssemblyService _assemblyService;
    private readonly ILogger<AssemblyController> _logger;

    public AssemblyController(IAssemblyService assemblyService, ILogger<AssemblyController> logger)
    {
        _assemblyService = assemblyService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of assembly constituencies
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<AssemblyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssemblyResponse>>> GetAssemblyList([FromQuery] bool activeOnly = true)
    {
        try
        {
            var result = await _assemblyService.GetAllAssembliesAsync(activeOnly);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assembly list");
            return StatusCode(500, new { message = "An error occurred while retrieving assembly list" });
        }
    }

    /// <summary>
    /// Get assembly by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssemblyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssemblyResponse>> GetAssembly(int id)
    {
        try
        {
            var result = await _assemblyService.GetAssemblyByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = $"Assembly with ID {id} not found" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assembly {AssemblyId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving assembly" });
        }
    }
}
