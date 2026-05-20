using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.IRepository;

public interface IAdminMembersRepository
{
    Task<ResponseModel<PagedResponse<AdminMemberListItemResponse>>> GetMembersAsync(
        int clientId,
        int page,
        int pageSize,
        string? search,
        int? firmId,
        int? planId,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo,
        string? sortBy,
        string? sortOrder,
        CancellationToken cancellationToken = default);

    Task<ResponseModel<AdminMemberDetailResponse>> GetMemberDetailAsync(int clientId, int id, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> VerifyMemberAsync(int clientId, int id, string notes, int changedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> HoldMemberAsync(int clientId, int id, string reason, int changedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> RejectMemberAsync(int clientId, int id, string feedback, int changedBy, CancellationToken cancellationToken = default);
}
