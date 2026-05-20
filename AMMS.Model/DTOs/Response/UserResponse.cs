using MVEA.Model.Enums;

namespace MVEA.Model.DTOs.Response;

public class UserResponse
{
    public int Id { get; set; }        
    public DateTime? LastLoginAt { get; set; }    
}
