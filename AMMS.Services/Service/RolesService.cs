using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;

    public RolesService(IRolesRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }

    public Task<ResponseModel<IReadOnlyList<RoleRowResponse>>> GetRolesAsync(int clientId, CancellationToken cancellationToken = default)
        => _rolesRepository.GetRolesAsync(clientId, cancellationToken);

    public Task<ResponseModel<bool>> UpdatePermissionsAsync(int clientId, string roleName, UpdateRolePermissionsRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "role is required.", ErrorId = -1 });
        }

        return _rolesRepository.UpdatePermissionsAsync(clientId, roleName, request, modifiedBy, cancellationToken);
    }
}
