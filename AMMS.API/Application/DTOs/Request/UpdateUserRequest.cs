namespace MVEA.Application.DTOs.Request;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsTwoFactorEnabled { get; set; }
}
