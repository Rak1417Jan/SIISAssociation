
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.Interfaces;

public interface IUserService
{
    Task<ResponseModel<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    
}
