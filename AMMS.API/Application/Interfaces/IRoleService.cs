using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default);
}
