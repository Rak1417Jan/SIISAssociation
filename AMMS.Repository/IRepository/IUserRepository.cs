using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.Interfaces;

public interface IUserRepository 
{
    Task<ResponseModel<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    
}
