using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
}
