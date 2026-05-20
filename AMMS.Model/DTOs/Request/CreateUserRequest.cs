using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Request;

public class CreateUserRequest
{
    public int ClientId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Password { get; set; }=string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    
}
