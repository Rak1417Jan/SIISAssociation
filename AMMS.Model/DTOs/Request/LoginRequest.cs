using System.ComponentModel.DataAnnotations;

namespace MVEA.Model.DTOs.Request;

public class OTPLoginRequest
{
    public string MobileNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Otp { get; set; }
    public bool UseOtp { get; set; }
}
public class LoginRequest
{
    public int ClientId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; }=string.Empty;
}
public class SendOtpRequest
{
    [Required]
    public string MobileNumber { get; set; } = string.Empty;
}
public class RefreshTokenRequest
{
    public string? AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}