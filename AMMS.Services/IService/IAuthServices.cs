using MVEA.API.Application.DTOs.Response;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MVEA.Services.IService
{
    public interface IAuthService
    {
        Task<ResponseModel<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<ResponseModel<OtpResponse>> SendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default);
        Task<ResponseModel<OtpResponse>> ResendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default);
        Task<ResponseModel<OtpValidationResponse>> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default);
        Task<bool> LogoutAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<ResponseModel<SessionInfoResponse>> GetSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
        Task<ResponseModel<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest refreshToken, CancellationToken cancellationToken = default);
        Task<ResponseModel<AdminPasswordAckResponse>> RequestAdminStaffPasswordResetAsync(AdminStaffPasswordResetRequest request, CancellationToken cancellationToken = default);
        Task<ResponseModel<AdminPasswordAckResponse>> CompleteAdminStaffPasswordChangeAsync(AdminStaffPasswordChangeRequest request, CancellationToken cancellationToken = default);
    }
}
