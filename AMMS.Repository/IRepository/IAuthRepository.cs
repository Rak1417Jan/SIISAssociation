using MVEA.API.Application.DTOs.Response;
using MVEA.Model.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Repository.IRepository
{
    public interface IAuthRepository
    {
        Task<ResponseModel<OtpResponse>> SendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default);
        Task<ResponseModel<OtpResponse>> ResendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default);
        Task<ResponseModel<OtpValidationResponse>> ValidateOtpAsync(string mobileNumber, string otpCode, CancellationToken cancellationToken = default);
        Task<ResponseModel<LoginResponse>> ValidateLoginAsync(string username, string password, int clientId, CancellationToken cancellationToken = default);
        Task<ResponseModel<StaffPasswordResetIssueResult>> RequestStaffPasswordResetAsync(int clientId, string? email, string? username, byte[] tokenHash, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
        Task<ResponseModel<bool>> CompleteStaffPasswordResetAsync(byte[] tokenHash, byte[] passwordHash, byte[] passwordSalt, CancellationToken cancellationToken = default);
        Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiresOn, CancellationToken cancellationToken = default);
        Task<UserInfo?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task ClearRefreshTokensForUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
