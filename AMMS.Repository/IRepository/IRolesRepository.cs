using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.IRepository;

public interface IRolesRepository
{
    Task<ResponseModel<IReadOnlyList<RoleRowResponse>>> GetRolesAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdatePermissionsAsync(int clientId, string roleName, UpdateRolePermissionsRequest request, int modifiedBy, CancellationToken cancellationToken = default);
}
