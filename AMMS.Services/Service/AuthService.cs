
using AMMS.Model.DTOs.Request;
using Azure;
using Microsoft.Extensions.Logging;
using MVEA.API.Application.DTOs.Response;
using MVEA.Comman;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Repository.UnitOfWork;
using MVEA.Services.IService;
using MVEA.Services.Messaging;
using MVEA.Services.Service;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MVEA.Services.Services;

/// <summary>
/// Authentication service - Example implementation showing Unit of Work pattern
/// </summary>
public class AuthService : IAuthService
{
    private const string PasswordResetAckMessage =
        "If the account exists, password reset instructions have been sent.";

    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;
    private readonly ITokenService _tokenService;
    private readonly IStaffPasswordResetNotifier _passwordResetNotifier;
    private readonly ITokenDenylistRepository _tokenDenylistRepository;

    public AuthService(
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger,
        ITokenService tokenService,
        IStaffPasswordResetNotifier passwordResetNotifier,
        ITokenDenylistRepository tokenDenylistRepository)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _tokenService = tokenService;
        _passwordResetNotifier = passwordResetNotifier;
        _tokenDenylistRepository = tokenDenylistRepository;
    }

    public async Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _unitOfWork.AuthRepository.ValidateLoginAsync(request.UserName, request.Password, request.ClientId, cancellationToken);
        if (response != null && string.IsNullOrEmpty(response.ErrorMessage))
        {
            // Generate tokens            
            response.Data!.AccessToken = _tokenService.GenerateJwtToken(response.Data);
            response.Data!.RefreshToken = _tokenService.GenerateRefreshToken();
            try
            {
                DateTime refreshExpires = DateTime.UtcNow.Add(RefreshTokenLifetime);
                await _unitOfWork.AuthRepository.SaveRefreshTokenAsync(
                    response.Data.UserId,
                    response.Data.RefreshToken,
                    refreshExpires,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist refresh token for user {UserId}.", response.Data.UserId);
                return new ResponseModel<LoginResponse>()
                {
                    ErrorMessage = "Login succeeded but refresh token could not be saved.",
                    ErrorId = -1
                };
            }
        }
        return response ?? new ResponseModel<LoginResponse>() { ErrorMessage = "Not able to validate user." };
    }
    public async Task<ResponseModel<OtpResponse>> SendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            _logger.LogWarning("SendOtpAsync called with empty mobile number.");
            return new ResponseModel<OtpResponse>() { ErrorMessage = "SendOtpAsync called with empty mobile number.", ErrorId = -1 };
        }

        try
        {
            return await _unitOfWork.AuthRepository.SendOtpAsync(mobileNumber, cancellationToken);

            // TODO: Replace the logging above with a call to your SMS provider:
            // await _smsService.SendAsync(mobileNumber, $"Your code is {code}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending OTP to {MobileNumber}.", mobileNumber);
            return new ResponseModel<OtpResponse>() { ErrorMessage = "Error while sending OTP to {MobileNumber}.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<OtpResponse>> ResendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            _logger.LogWarning("ResendOtpAsync called with empty mobile number.");
            return new ResponseModel<OtpResponse>() { ErrorMessage = "Mobile number is required.", ErrorId = -1 };
        }

        try
        {
            return await _unitOfWork.AuthRepository.ResendOtpAsync(mobileNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while resending OTP to {MobileNumber}.", mobileNumber);
            return new ResponseModel<OtpResponse>() { ErrorMessage = "Error while resending OTP.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<OtpValidationResponse>> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default)
    {
        // TODO: Implement OTP verification logic
        var response = await _unitOfWork.AuthRepository.ValidateOtpAsync(mobileNumber, otp, cancellationToken);
        if (response != null && string.IsNullOrEmpty(response.ErrorMessage))
        {
            // Generate tokens            
            response.Data!.AccessToken = _tokenService.GenerateJwtToken(mobileNumber);
            response.Data!.RefreshToken = _tokenService.GenerateRefreshToken();
        }
        return response ?? new ResponseModel<OtpValidationResponse>() { ErrorMessage = "Not able to validate user." };
    }
    public async Task<ResponseModel<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        //var response = await _unitOfWork.AuthRepository.RegisterUserAsync(request, cancellationToken);

        //if (response != null && string.IsNullOrEmpty(response.ErrorMessage))
        //{
        //    response.Data!.AccessToken = _tokenService.GenerateJwtToken(response.Data);
        //    response.Data!.RefreshToken = _tokenService.GenerateRefreshToken();

        //    await _unitOfWork.AuthRepository.SaveRefreshTokenAsync(
        //        response.Data.UserId,
        //        response.Data.RefreshToken,
        //        cancellationToken);

        //    await _unitOfWork.SaveChangesAsync(cancellationToken);
        //}

        return  new ResponseModel<LoginResponse> { ErrorMessage = "Registration failed." };
    }
    public async Task<ResponseModel<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken == null || string.IsNullOrWhiteSpace(refreshToken.RefreshToken))
        {
            return new ResponseModel<LoginResponse>()
            {
                ErrorMessage = "Refresh token is required.",
                ErrorId = -1
            };
        }

        try
        {
            UserInfo? user = await _unitOfWork.AuthRepository.ValidateRefreshTokenAsync(
                refreshToken.RefreshToken.Trim(),
                cancellationToken);

            if (user == null)
            {
                return new ResponseModel<LoginResponse>()
                {
                    ErrorMessage = "Invalid or expired refresh token.",
                    ErrorId = -1
                };
            }

            var loginResponse = new LoginResponse
            {
                UserId = user.UserId,
                ClientId = user.ClientId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };

            loginResponse.AccessToken = _tokenService.GenerateJwtToken(loginResponse);
            loginResponse.RefreshToken = _tokenService.GenerateRefreshToken();

            DateTime refreshExpires = DateTime.UtcNow.Add(RefreshTokenLifetime);
            await _unitOfWork.AuthRepository.SaveRefreshTokenAsync(
                loginResponse.UserId,
                loginResponse.RefreshToken,
                refreshExpires,
                cancellationToken);

            return new ResponseModel<LoginResponse>() { Data = loginResponse };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshTokenAsync failed.");
            return new ResponseModel<LoginResponse>()
            {
                ErrorMessage = "Unable to refresh session.",
                ErrorId = -1
            };
        }
    }
    public async Task<bool> LogoutAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        try
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(accessToken))
            {
                return false;
            }

            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);
            string? userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId) || userId < 1)
            {
                _logger.LogWarning("Logout: JWT has no UserId claim; cannot revoke refresh tokens or staff denylist.");
                return false;
            }

            await _tokenDenylistRepository.AddAsync(userId, "*", "Logout", cancellationToken);
            await _unitOfWork.AuthRepository.ClearRefreshTokensForUserAsync(userId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Logout failed while revoking session.");
            return false;
        }
    }

    public Task<ResponseModel<SessionInfoResponse>> GetSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(new ResponseModel<SessionInfoResponse>
            {
                ErrorMessage = "Not authenticated.",
                ErrorId = -1
            });
        }

        int? userId = null;
        string? userIdRaw = principal.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdRaw, out int uid))
        {
            userId = uid;
        }

        int? clientId = null;
        string? clientIdRaw = principal.FindFirst("ClientId")?.Value;
        if (int.TryParse(clientIdRaw, out int cid))
        {
            clientId = cid;
        }

        int? memberId = null;
        string? memberIdRaw = principal.FindFirst("MemberId")?.Value;
        if (int.TryParse(memberIdRaw, out int mid))
        {
            memberId = mid;
        }

        string username =
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.Identity?.Name
            ?? string.Empty;

        string role =
            principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("role")?.Value
            ?? string.Empty;

        string? mobile =
            principal.FindFirst("MobileNo")?.Value
            ?? principal.FindFirst("MobileNumber")?.Value;

        SessionInfoResponse session = new SessionInfoResponse
        {
            UserId = userId,
            ClientId = clientId,
            Username = username,
            FullName = principal.FindFirst("FullName")?.Value ?? string.Empty,
            Role = role,
            MobileNumber = mobile,
            MemberId = memberId,
            IsActive = true
        };

        return Task.FromResult(new ResponseModel<SessionInfoResponse> { Data = session });
    }

    public async Task<ResponseModel<AdminPasswordAckResponse>> RequestAdminStaffPasswordResetAsync(
        AdminStaffPasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        string? normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        string? normalizedUsername = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username.Trim();

        if (normalizedEmail == null && normalizedUsername == null)
        {
            return new ResponseModel<AdminPasswordAckResponse>
            {
                ErrorMessage = "Email or username is required.",
                ErrorId = -1
            };
        }

        try
        {
            string tokenPlain = BuildUrlSafeResetToken();
            byte[] tokenHash;
            using (SHA256 sha = SHA256.Create())
            {
                tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(tokenPlain));
            }

            DateTime expiresAt = DateTime.UtcNow.AddHours(1);
            ResponseModel<StaffPasswordResetIssueResult> repoResult =
                await _unitOfWork.AuthRepository.RequestStaffPasswordResetAsync(
                    request.ClientId,
                    normalizedEmail,
                    normalizedUsername,
                    tokenHash,
                    expiresAt,
                    cancellationToken);

            if (!string.IsNullOrEmpty(repoResult.ErrorMessage))
            {
                return new ResponseModel<AdminPasswordAckResponse>
                {
                    ErrorMessage = repoResult.ErrorMessage,
                    ErrorId = repoResult.ErrorId
                };
            }

            if (repoResult.Data?.Issued == true && !string.IsNullOrWhiteSpace(repoResult.Data.Email))
            {
                await _passwordResetNotifier.NotifyResetIssuedAsync(
                    repoResult.Data.Email,
                    tokenPlain,
                    request.ClientId,
                    cancellationToken);
            }

            return new ResponseModel<AdminPasswordAckResponse>
            {
                Data = new AdminPasswordAckResponse { Message = PasswordResetAckMessage }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RequestAdminStaffPasswordReset failed for client {ClientId}.", request.ClientId);
            return new ResponseModel<AdminPasswordAckResponse>
            {
                ErrorMessage = "Unable to request password reset.",
                ErrorId = -1
            };
        }
    }

    public async Task<ResponseModel<AdminPasswordAckResponse>> CompleteAdminStaffPasswordChangeAsync(
        AdminStaffPasswordChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new ResponseModel<AdminPasswordAckResponse>
            {
                ErrorMessage = "Token is required.",
                ErrorId = -1
            };
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return new ResponseModel<AdminPasswordAckResponse>
            {
                ErrorMessage = "Password must be at least 8 characters.",
                ErrorId = -1
            };
        }

        try
        {
            byte[] tokenHash;
            using (SHA256 sha = SHA256.Create())
            {
                tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(request.Token.Trim()));
            }

            CommandMethods.PasswordHashResult hashResult = CommandMethods.ConvertToHashResult(request.NewPassword);
            ResponseModel<bool> repoResult = await _unitOfWork.AuthRepository.CompleteStaffPasswordResetAsync(
                tokenHash,
                hashResult.PasswordHash,
                hashResult.PasswordSalt,
                cancellationToken);

            if (!string.IsNullOrEmpty(repoResult.ErrorMessage))
            {
                return new ResponseModel<AdminPasswordAckResponse>
                {
                    ErrorMessage = repoResult.ErrorMessage,
                    ErrorId = repoResult.ErrorId
                };
            }

            return new ResponseModel<AdminPasswordAckResponse>
            {
                Data = new AdminPasswordAckResponse { Message = "Your password has been updated." }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CompleteAdminStaffPasswordChange failed.");
            return new ResponseModel<AdminPasswordAckResponse>
            {
                ErrorMessage = "Unable to change password.",
                ErrorId = -1
            };
        }
    }

    private static string BuildUrlSafeResetToken()
    {
        byte[] raw = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(raw).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
