using AMMS.Model.DTOs.Request;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using MVEA.API.Application.DTOs.Response;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;


namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Send OTP to registered mobile number for login or registration
    /// </summary>
    [HttpPost("/otp/send")]
    public async Task<ActionResult<OtpResponse>> SendOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SendOtpAsync(request.MobileNumber);

        if (!result.Success)
            return BadRequest("Unable to send OTP.");

        return Ok(result);
    }

    /// <summary>
    /// Resend OTP for the same mobile number. Enforces a 60-second cooldown since the last OTP was generated for that number.
    /// </summary>
    [HttpPost("/auth/otp/resend")]
    public async Task<ActionResult<ResponseModel<OtpResponse>>> ResendOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.ResendOtpAsync(request.MobileNumber);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verify OTP and generate temporary authentication context
    /// </summary>
    [HttpPost("/otp/verify")]
    public async Task<ActionResult<AuthResponse>> VerifyOtp([FromBody] OTPLoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyOtpAsync(
            request.MobileNumber,
            request.Otp ?? string.Empty);

        if (result == null)
            return Unauthorized("Invalid OTP.");

        return Ok(result);
    }

    /// <summary>
    /// Request a staff password reset (no JWT). Response message is the same whether the account exists.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/auth/admin/password-reset")]
    public async Task<ActionResult<ResponseModel<AdminPasswordAckResponse>>> AdminStaffPasswordReset(
        [FromBody] AdminStaffPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ResponseModel<AdminPasswordAckResponse> result =
            await _authService.RequestAdminStaffPasswordResetAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Set a new staff password using the opaque token from the reset flow (no JWT).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/auth/admin/password-change")]
    public async Task<ActionResult<ResponseModel<AdminPasswordAckResponse>>> AdminStaffPasswordChange(
        [FromBody] AdminStaffPasswordChangeRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ResponseModel<AdminPasswordAckResponse> result =
            await _authService.CompleteAdminStaffPasswordChangeAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized("Invalid credentials.");

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ResponseModel<LoginResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ResponseModel<LoginResponse> result = await _authService.RefreshTokenAsync(request, cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    //[HttpPost("register")]
    //public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    var result = await _authService.RegisterAsync(request);        
    //    if (result == null)
    //        return BadRequest("Registration failed.");

    //    return Ok(result);
    //}

    //[HttpPost("forgot-password")]
    //public async Task<ActionResult<bool>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    //{
    //    var result = await _authService.ForgotPasswordAsync(request.Email);

    //    return Ok(result);
    //}

    //[HttpPost("reset-password")]
    //public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetPasswordRequest request)
    //{
    //    var result = await _authService.ResetPasswordAsync(request);

    //    if (!result)
    //        return BadRequest("Reset failed.");

    //    return Ok(true);
    //}

    /// <summary>
    /// Revoke the current staff session: denylist all JWTs for the user (same pattern as role change) and remove stored refresh tokens.
    /// </summary>
    [Authorize]
    [HttpPost("/auth/logout")]
    public async Task<ActionResult<object>> Logout(CancellationToken cancellationToken)
    {
        string token = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.Ordinal).Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { message = "Token missing." });
        }

        bool result = await _authService.LogoutAsync(token, cancellationToken);
        if (!result)
        {
            return BadRequest(new { message = "Unable to revoke session. Token must include a UserId claim (staff/admin JWT)." });
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Return role and identity claims for the current JWT (must be valid and not denylisted).
    /// </summary>
    [Authorize]
    [HttpGet("/auth/session")]
    public async Task<ActionResult<ResponseModel<SessionInfoResponse>>> GetSession(CancellationToken cancellationToken)
    {
        ResponseModel<SessionInfoResponse> result = await _authService.GetSessionAsync(User, cancellationToken);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
