using Microsoft.AspNetCore.Mvc;
using MVEA.Application.Services;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.Interfaces;

namespace MVEA.API.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IMasterService _masterService;
        private readonly ILogger<MasterController> _logger;

        public MasterController(IMasterService masterService, ILogger<MasterController> logger)
        {
            _masterService = masterService;
            _logger = logger;
        }

        [HttpPost("master")]
        [ProducesResponseType(typeof(IList<MasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IList<MasterResponse>>> GetMaster([FromBody] MasterRequest request)
        {
            try
            {
                var result = await _masterService.GetMasterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting master records");
                return StatusCode(500, new { message = "An error occurred while getting master records" });
            }
        }
    }
}
