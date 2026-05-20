using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface IAdminFirmsService
{
    Task<ResponseModel<PagedResponse<FirmListItemResponse>>> GetFirmsAsync(int clientId, int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<ResponseModel<FirmDetailResponse>> GetFirmDetailAsync(int clientId, int id, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateFirmAsync(int clientId, CreateFirmRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateFirmAsync(int clientId, int id, UpdateFirmRequest request, int modifiedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> SoftDeleteFirmAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UploadDocumentAsync(int clientId, int id, string documentType, string blobUrl, int uploadedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> LinkMemberAsync(int clientId, int id, LinkFirmMemberRequest request, int linkedBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UnlinkMemberAsync(int clientId, int id, int memberId, int unlinkedBy, CancellationToken cancellationToken = default);
}
