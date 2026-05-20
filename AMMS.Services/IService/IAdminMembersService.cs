using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IAdminMembersService
{
    Task<ResponseModel<PagedResponse<AdminMemberListItemResponse>>> GetMembersAsync(
        int clientId,
        int page,
        int pageSize,
        string? status,
        int? firmId,
        int? planId,
        string? search,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken = default);

    Task<ResponseModel<AdminMemberDetailResponse>> GetMemberDetailAsync(int clientId, int id, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> VerifyMemberAsync(int clientId, int id, VerifyMemberRequest request, int changedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> HoldMemberAsync(int clientId, int id, HoldMemberRequest request, int changedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> RejectMemberAsync(int clientId, int id, RejectMemberRequest request, int changedBy, CancellationToken cancellationToken = default);
}
