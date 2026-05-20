using AMMS.Model.DTOs.Request;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MVEA.API.Application.DTOs.Response;
using MVEA.Comman;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Repository.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ISqlConnectionFactory _connection;
        private readonly ILogger<AuthRepository> _logger;
        public AuthRepository(ISqlConnectionFactory connection, ILogger<AuthRepository> logger)
        {
            _connection = connection;
            _logger = logger;
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
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in SendOtpAsync.");
                    return new ResponseModel<OtpResponse>() { ErrorMessage = "Database connection is not available in SendOtpAsync.", ErrorId = -1 };
                }

                // Prepare Dapper command with ambient transaction and cancellation support
                var parameters = new { MobileNo = mobileNumber };

                var otpResponse = await connection.QueryAsync<OtpResponse>("sp_GenerateOTP", parameters, commandType: CommandType.StoredProcedure);

                var otpModel = otpResponse.FirstOrDefault();

                if (otpModel == null || string.IsNullOrEmpty(otpModel.OtpCode) || otpModel.ExpiresOn == null || otpModel.ExpiresOn <= DateTime.UtcNow)
                {
                    _logger.LogInformation("No valid OTP returned for mobile {MobileNumber}.", mobileNumber);
                    return new ResponseModel<OtpResponse>() { ErrorMessage = "No valid OTP returned for mobile {MobileNumber}.", ErrorId = -1 };

                }

                // Mask OTP for logs (show only last 2-4 digits depending on length)
                var code = otpModel.OtpCode!;
                var masked = code.Length <= 4 ? new string('*', code.Length) : "****" + code[^4..];
                _logger.LogInformation("sp_GenerateOTP returned OTP_ID={OtpId} for {MobileNumber}: {MaskedOtp} (expires at {ExpiresAt} UTC).",
                    otpModel.OtpId, mobileNumber, masked, otpModel.ExpiresOn);

                // TODO: Replace the logging above with a call to your SMS provider:
                // await _smsService.SendAsync(mobileNumber, $"Your code is {code}", cancellationToken);

                return new ResponseModel<OtpResponse>() { Data = otpModel };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SendOtpAsync cancelled for mobile {MobileNumber}.", mobileNumber);
                return new ResponseModel<OtpResponse>() { ErrorMessage = "SendOtpAsync cancelled for mobile {MobileNumber}.", ErrorId = -1 };
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
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in ResendOtpAsync.");
                    return new ResponseModel<OtpResponse>() { ErrorMessage = "Database connection is not available.", ErrorId = -1 };
                }

                var parameters = new { MobileNo = mobileNumber };
                var otpResponse = await connection.QueryAsync<OtpResponse>("sp_ResendOTP", parameters, commandType: CommandType.StoredProcedure );
                var otpModel = otpResponse.FirstOrDefault();

                if (otpModel == null || string.IsNullOrEmpty(otpModel.OtpCode) || otpModel.ExpiresOn == null || otpModel.ExpiresOn <= DateTime.UtcNow)
                {
                    _logger.LogInformation("No valid OTP returned on resend for mobile {MobileNumber}.", mobileNumber);
                    return new ResponseModel<OtpResponse>() { ErrorMessage = "Unable to resend OTP.", ErrorId = -1 };
                }

                var code = otpModel.OtpCode!;
                var masked = code.Length <= 4 ? new string('*', code.Length) : "****" + code[^4..];
                _logger.LogInformation("sp_ResendOTP returned OTP_ID={OtpId} for {MobileNumber}: {MaskedOtp} (expires at {ExpiresAt} UTC).",
                    otpModel.OtpId, mobileNumber, masked, otpModel.ExpiresOn);

                return new ResponseModel<OtpResponse>() { Data = otpModel };
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                return new ResponseModel<OtpResponse>() { ErrorMessage = ex.Message, ErrorId = -1 };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ResendOtpAsync cancelled for mobile {MobileNumber}.", mobileNumber);
                return new ResponseModel<OtpResponse>() { ErrorMessage = "Request cancelled.", ErrorId = -1 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resending OTP to {MobileNumber}.", mobileNumber);
                return new ResponseModel<OtpResponse>() { ErrorMessage = "Error while resending OTP.", ErrorId = -1 };
            }
        }

        public async Task<ResponseModel<OtpValidationResponse>> ValidateOtpAsync(string mobileNumber, string otpCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(otpCode))
            {
                _logger.LogWarning("ValidateOtpAsync called with empty parameters. Mobile={MobileNumber}, OTP={OtpCode}", mobileNumber, otpCode);
                return new ResponseModel<OtpValidationResponse>()
                {
                    ErrorMessage = "Mobile number and OTP are required.",
                    ErrorId = -1
                };
            }

            try
            {
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in ValidateOtpAsync.");
                    return new ResponseModel<OtpValidationResponse>()
                    {
                        ErrorMessage = "Database connection is not available in ValidateOtpAsync.",
                        ErrorId = -1
                    };
                }

                // Prepare parameters for stored procedure
                var parameters = new { MobileNo = mobileNumber, OTPCode = otpCode };

                var result = await connection.QueryAsync<OtpValidationResponse>(
                    "sp_ValidateOTP",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var validation = result.FirstOrDefault();

                if (validation == null || validation.IsValid == false)
                {
                    _logger.LogInformation("Invalid or expired OTP for mobile {MobileNumber}.", mobileNumber);
                    return new ResponseModel<OtpValidationResponse>()
                    {
                        ErrorMessage = "Invalid or expired OTP.",
                        ErrorId = -1
                    };
                }

                _logger.LogInformation("OTP validated successfully for mobile {MobileNumber}.", mobileNumber);

                return new ResponseModel<OtpValidationResponse>() { Data = validation };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ValidateOtpAsync cancelled for mobile {MobileNumber}.", mobileNumber);
                return new ResponseModel<OtpValidationResponse>()
                {
                    ErrorMessage = "ValidateOtpAsync cancelled.",
                    ErrorId = -1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating OTP for {MobileNumber}.", mobileNumber);
                return new ResponseModel<OtpValidationResponse>()
                {
                    ErrorMessage = "Error while validating OTP.",
                    ErrorId = -1
                };
            }
        }
        public async Task<ResponseModel<LoginResponse>> ValidateLoginAsync(string username, string password, int clientId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || clientId < 1)
            {
                _logger.LogWarning("ValidateLoginAsync called with empty parameters. Username={Username}", username);
                return new ResponseModel<LoginResponse>()
                {
                    ErrorMessage = "Username, password, and client are required.",
                    ErrorId = -1
                };
            }

            try
            {
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in ValidateLoginAsync.");
                    return new ResponseModel<LoginResponse>()
                    {
                        ErrorMessage = "Database connection is not available in ValidateLoginAsync.",
                        ErrorId = -1
                    };
                }

                var parameters = new { Username = username, ClientId = clientId };

                var userRecord = await connection.QueryAsync<UserInfo>(
                    "sp_GetUserByUsername",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var user = userRecord.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogInformation("No user found for Username={Username}.", username);
                    return new ResponseModel<LoginResponse>()
                    {
                        ErrorMessage = "Invalid username or password.",
                        ErrorId = -1
                    };
                }

                // Verify password hash
                bool isValid = CommandMethods.ValidatePassword(password, new CommandMethods.PasswordHashResult() { PasswordHash = user.PasswordHash!, PasswordSalt = user.PasswordSalt! });
                if (!isValid)
                {
                    _logger.LogInformation("Invalid password for Username={Username}.", username);
                    return new ResponseModel<LoginResponse>()
                    {
                        ErrorMessage = "Invalid username or password.",
                        ErrorId = -1
                    };
                }

                _logger.LogInformation("User {Username} validated successfully.", username);

                var loginResponse = new LoginResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role,
                    ClientId = user.ClientId
                };

                return new ResponseModel<LoginResponse>() { Data = loginResponse };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ValidateLoginAsync cancelled for Username={Username}.", username);
                return new ResponseModel<LoginResponse>()
                {
                    ErrorMessage = "ValidateLoginAsync cancelled.",
                    ErrorId = -1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while validating login for Username={Username}.", username);
                return new ResponseModel<LoginResponse>()
                {
                    ErrorMessage = "Error while validating login.",
                    ErrorId = -1
                };
            }
        }

        public async Task<ResponseModel<int>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    return new ResponseModel<int>
                    {
                        ErrorMessage = "Database connection not available.",
                        ErrorId = -1
                    };
                }

                var parameters = new DynamicParameters();

                parameters.Add("@OWNER_NAME", request.OwnerName);
                parameters.Add("@EMAIL", request.Email);
                parameters.Add("@MOBILE_NUMBER", request.MobileNumber);
                parameters.Add("@PASSWORD_HASH", request.Password); // already hashed before calling repo
                parameters.Add("@COMPANY_ID", request.CompanyId);
                parameters.Add("@COMPANY_NAME", request.CompanyName);
                parameters.Add("@ADDRESS", request.Address);
                parameters.Add("@CITY", request.City);
                parameters.Add("@PLAN_ID", request.PlanId);
                parameters.Add("@CREATED_BY", 1);
                parameters.Add("@NEW_USER_ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "sp_RegisterUser",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var newUserId = parameters.Get<int>("@NEW_USER_ID");

                return new ResponseModel<int> { Data = newUserId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration.");
                return new ResponseModel<int>
                {
                    ErrorMessage = "Registration failed.",
                    ErrorId = -1
                };
            }
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiresOn, CancellationToken cancellationToken = default)
        {
            var connection = _connection.GetConnection();
            if (connection == null)
            {
                _logger.LogError("Database connection is not available in SaveRefreshTokenAsync.");
                throw new InvalidOperationException("Database connection is not available.");
            }

            var parameters = new
            {
                USER_ID = userId,
                REFRESH_TOKEN = refreshToken,
                EXPIRES_ON = expiresOn
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_SaveRefreshToken",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<UserInfo?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var connection = _connection.GetConnection();
            if (connection == null)
            {
                _logger.LogError("Database connection is not available in ValidateRefreshTokenAsync.");
                return null;
            }

            var result = await connection.QueryAsync<UserInfo>(
                new CommandDefinition(
                    "sp_ValidateRefreshToken",
                    new { REFRESH_TOKEN = refreshToken },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return result.FirstOrDefault();
        }

        public async Task ClearRefreshTokensForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (userId < 1)
            {
                return;
            }

            var connection = _connection.GetConnection();
            if (connection == null)
            {
                _logger.LogError("Database connection is not available in ClearRefreshTokensForUserAsync.");
                throw new InvalidOperationException("Database connection is not available.");
            }

            await connection.ExecuteAsync(
                new CommandDefinition(
                    "sp_ClearUserRefreshTokens",
                    new { USER_ID = userId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> LogoutAsync(string token)
        {
            var connection = _connection.GetConnection();

            await connection.ExecuteAsync(
                "sp_LogoutUser",
                new { ACCESS_TOKEN = token },
                commandType: CommandType.StoredProcedure);

            return true;
        }

        public async Task<ResponseModel<StaffPasswordResetIssueResult>> RequestStaffPasswordResetAsync(
            int clientId,
            string? email,
            string? username,
            byte[] tokenHash,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            if (tokenHash == null || tokenHash.Length != 32)
            {
                return new ResponseModel<StaffPasswordResetIssueResult>
                {
                    ErrorMessage = "Invalid token material.",
                    ErrorId = -1
                };
            }

            try
            {
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in RequestStaffPasswordResetAsync.");
                    return new ResponseModel<StaffPasswordResetIssueResult>
                    {
                        ErrorMessage = "Database connection is not available.",
                        ErrorId = -1
                    };
                }

                var parameters = new
                {
                    ClientId = clientId,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    Username = string.IsNullOrWhiteSpace(username) ? null : username.Trim(),
                    TokenHash = tokenHash,
                    ExpiresAt = expiresAtUtc
                };

                var row = await connection.QueryFirstOrDefaultAsync<StaffPasswordResetIssueResult>(
                    new CommandDefinition(
                        "sp_Admin_RequestStaffPasswordReset",
                        parameters,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));

                return new ResponseModel<StaffPasswordResetIssueResult>
                {
                    Data = row ?? new StaffPasswordResetIssueResult { Issued = false }
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "RequestStaffPasswordReset failed for client {ClientId}.", clientId);
                return new ResponseModel<StaffPasswordResetIssueResult>
                {
                    ErrorMessage = ex.Message,
                    ErrorId = ex.Number
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestStaffPasswordReset failed for client {ClientId}.", clientId);
                return new ResponseModel<StaffPasswordResetIssueResult>
                {
                    ErrorMessage = "Unable to request password reset.",
                    ErrorId = -1
                };
            }
        }

        public async Task<ResponseModel<bool>> CompleteStaffPasswordResetAsync(
            byte[] tokenHash,
            byte[] passwordHash,
            byte[] passwordSalt,
            CancellationToken cancellationToken = default)
        {
            if (tokenHash == null || tokenHash.Length != 32)
            {
                return new ResponseModel<bool> { ErrorMessage = "Invalid token.", ErrorId = -1 };
            }

            if (passwordHash == null || passwordHash.Length == 0 || passwordSalt == null || passwordSalt.Length == 0)
            {
                return new ResponseModel<bool> { ErrorMessage = "Invalid password material.", ErrorId = -1 };
            }

            try
            {
                var connection = _connection.GetConnection();
                if (connection == null)
                {
                    _logger.LogError("Database connection is not available in CompleteStaffPasswordResetAsync.");
                    return new ResponseModel<bool>
                    {
                        ErrorMessage = "Database connection is not available.",
                        ErrorId = -1
                    };
                }

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        "sp_Admin_CompleteStaffPasswordReset",
                        new
                        {
                            TokenHash = tokenHash,
                            PasswordHash = passwordHash,
                            PasswordSalt = passwordSalt,
                            ModifiedBy = (int?)null
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));

                return new ResponseModel<bool> { Data = true };
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(ex, "CompleteStaffPasswordReset failed.");
                return new ResponseModel<bool>
                {
                    ErrorMessage = ex.Message,
                    ErrorId = ex.Number
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompleteStaffPasswordReset failed.");
                return new ResponseModel<bool>
                {
                    ErrorMessage = "Unable to change password.",
                    ErrorId = -1
                };
            }
        }
    }
}
