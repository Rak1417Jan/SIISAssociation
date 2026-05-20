using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class AdminFirmsService : IAdminFirmsService
{
    private readonly IAdminFirmsRepository _adminFirmsRepository;

    public AdminFirmsService(IAdminFirmsRepository adminFirmsRepository)
    {
        _adminFirmsRepository = adminFirmsRepository;
    }

    public Task<ResponseModel<PagedResponse<FirmListItemResponse>>> GetFirmsAsync(int clientId, int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
        return _adminFirmsRepository.GetFirmsAsync(clientId, page, pageSize, search, cancellationToken);
    }

    public Task<ResponseModel<FirmDetailResponse>> GetFirmDetailAsync(int clientId, int id, CancellationToken cancellationToken = default)
        => _adminFirmsRepository.GetFirmDetailAsync(clientId, id, cancellationToken);

    public Task<ResponseModel<int>> CreateFirmAsync(int clientId, CreateFirmRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult(new ResponseModel<int> { ErrorMessage = "name is required.", ErrorId = -1 });
        }

        if (request.CompanyTypeId <= 0)
        {
            return Task.FromResult(new ResponseModel<int> { ErrorMessage = "companyTypeId is required.", ErrorId = -1 });
        }

        if (!string.IsNullOrWhiteSpace(request.GstNo) && request.GstNo.Trim().Length != 15)
        {
            return Task.FromResult(new ResponseModel<int> { ErrorMessage = "GST number must be 15 characters.", ErrorId = -1 });
        }

        return _adminFirmsRepository.CreateFirmAsync(clientId, request, createdBy, cancellationToken);
    }

    public Task<ResponseModel<bool>> UpdateFirmAsync(int clientId, int id, UpdateFirmRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "name is required.", ErrorId = -1 });
        }

        if (!string.IsNullOrWhiteSpace(request.GstNo) && request.GstNo.Trim().Length != 15)
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "GST number must be 15 characters.", ErrorId = -1 });
        }

        return _adminFirmsRepository.UpdateFirmAsync(clientId, id, request, modifiedBy, cancellationToken);
    }

    public Task<ResponseModel<bool>> SoftDeleteFirmAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default)
        => _adminFirmsRepository.SoftDeleteFirmAsync(clientId, id, modifiedBy, cancellationToken);

    public Task<ResponseModel<bool>> UploadDocumentAsync(int clientId, int id, string documentType, string blobUrl, int uploadedBy, CancellationToken cancellationToken = default)
        => _adminFirmsRepository.CreateFirmDocumentAsync(clientId, id, documentType, blobUrl, uploadedBy, cancellationToken);

    public Task<ResponseModel<bool>> LinkMemberAsync(int clientId, int id, LinkFirmMemberRequest request, int linkedBy, CancellationToken cancellationToken = default)
        => _adminFirmsRepository.LinkMemberAsync(clientId, id, request.MemberId, request.RoleInFirm, linkedBy, cancellationToken);

    public Task<ResponseModel<bool>> UnlinkMemberAsync(int clientId, int id, int memberId, int unlinkedBy, CancellationToken cancellationToken = default)
        => _adminFirmsRepository.UnlinkMemberAsync(clientId, id, memberId, unlinkedBy, cancellationToken);
}
