using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> SendOtpAsync(string mobileNumber, CancellationToken cancellationToken = default);
    Task<AuthResponse> VerifyOtpAsync(string mobileNumber, string otp, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string token, CancellationToken cancellationToken = default);
}
