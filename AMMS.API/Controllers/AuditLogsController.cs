using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;


namespace MVEA.API.Controllers;

[ApiController]
//[ApiVersion("1.0")]
//[Route("api/v{version:apiVersion}/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuditLogsController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Send OTP to registered mobile number for login or registration
    /// </summary>
    [HttpPost("send-otp")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult> SendOtp([FromBody] string mobileNumber)
    {
        var result = await _authService.SendOtpAsync(mobileNumber);
        return Ok(result);
    }

   
}
