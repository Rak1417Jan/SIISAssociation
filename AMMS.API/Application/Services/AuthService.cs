using Dapper;
using MVEA.API.Application.DTOs.Response;
using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using System.Data;
using System.Data.Common;

namespace MVEA.Application.Services;

/// <summary>
/// Authentication service - Example implementation showing Unit of Work pattern
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // TODO: Implement login logic with OTP/Password validation
            var user = await _userRepository.GetByMobileAsync(request.MobileNumber, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid mobile number");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // TODO: Generate JWT token
            return new AuthResponse
            {
                Token = "jwt_token_here",
                RefreshToken = "refresh_token_here",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserInfo
                {
                    Id = user.Id,
                    MobileNumber = user.MobileNumber,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

   
    public async Task<bool> SendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            _logger.LogWarning("SendOtpAsync called with empty mobile number.");
            return false;
        }

        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                _logger.LogError("Database connection is not available in SendOtpAsync.");
                return false;
            }

            // Prepare Dapper command with ambient transaction and cancellation support
            var parameters = new { MobileNumber = mobileNumber };
            var command = new CommandDefinition(
                commandText: "sp_GenerateOTP",
                parameters: parameters,
                transaction: _unitOfWork.Transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            // Query the stored procedure asynchronously using Dapper
            var result = await global::Dapper.SqlMapper.QueryAsync<OtpResponse>(connection, command).ConfigureAwait(false);
            var otpModel = result.FirstOrDefault();

            if (otpModel == null || string.IsNullOrEmpty(otpModel.OtpCode) || otpModel.ExpiresOn == null || otpModel.ExpiresOn <= DateTime.UtcNow)
            {
                _logger.LogInformation("No valid OTP returned for mobile {MobileNumber}.", mobileNumber);
                return false;
            }

            // Mask OTP for logs (show only last 2-4 digits depending on length)
            var code = otpModel.OtpCode!;
            var masked = code.Length <= 4 ? new string('*', code.Length) : "****" + code[^4..];
            _logger.LogInformation("sp_GenerateOTP returned OTP_ID={OtpId} for {MobileNumber}: {MaskedOtp} (expires at {ExpiresAt} UTC).",
                otpModel.OtpId, mobileNumber, masked, otpModel.ExpiresOn);

            // TODO: Replace the logging above with a call to your SMS provider:
            // await _smsService.SendAsync(mobileNumber, $"Your code is {code}", cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SendOtpAsync cancelled for mobile {MobileNumber}.", mobileNumber);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending OTP to {MobileNumber}.", mobileNumber);
            return false;
        }
    }   

    

    public async Task<AuthResponse> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default)
    {
        // TODO: Implement OTP verification logic
        return await LoginAsync(new LoginRequest { MobileNumber = mobileNumber, Otp = otp, UseOtp = true }, cancellationToken);
    }

    public async Task<bool> LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        // TODO: Implement logout logic (invalidate token)
        return await Task.FromResult(true);
    }


    /// <summary>
    /// Model representing OTP record returned by stored procedure sp_GenerateOTP
    /// </summary>


   
}
