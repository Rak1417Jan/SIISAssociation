using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IStaffService
{
    Task<ResponseModel<IReadOnlyList<StaffListItemResponse>>> GetStaffAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateStaffAsync(int clientId, CreateStaffRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateStaffAsync(int clientId, int id, UpdateStaffRequest request, int modifiedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> DeactivateStaffAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default);
}
