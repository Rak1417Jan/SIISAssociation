namespace MVEA.Application.DTOs.Request;

public class LoginRequest
{
    public string MobileNumber { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Otp { get; set; }
    public bool UseOtp { get; set; }
}
