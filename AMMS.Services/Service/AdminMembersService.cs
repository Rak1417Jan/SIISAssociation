using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class AdminMembersService : IAdminMembersService
{
    private readonly IAdminMembersRepository _adminMembersRepository;
    private readonly ILogger<AdminMembersService> _logger;

    public AdminMembersService(IAdminMembersRepository adminMembersRepository, ILogger<AdminMembersService> logger)
    {
        _adminMembersRepository = adminMembersRepository;
        _logger = logger;
    }

    public Task<ResponseModel<PagedResponse<AdminMemberListItemResponse>>> GetMembersAsync(
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
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        return _adminMembersRepository.GetMembersAsync(clientId, page, pageSize, search, firmId, planId, status, dateFrom, dateTo, sortBy, sortOrder, cancellationToken);
    }

    public Task<ResponseModel<AdminMemberDetailResponse>> GetMemberDetailAsync(int clientId, int id, CancellationToken cancellationToken = default)
    {
        return _adminMembersRepository.GetMemberDetailAsync(clientId, id, cancellationToken);
    }

    public Task<ResponseModel<bool>> VerifyMemberAsync(int clientId, int id, VerifyMemberRequest request, int changedBy, CancellationToken cancellationToken = default)
    {
        return _adminMembersRepository.VerifyMemberAsync(clientId, id, request.Notes ?? string.Empty, changedBy, cancellationToken);
    }

    public Task<ResponseModel<bool>> HoldMemberAsync(int clientId, int id, HoldMemberRequest request, int changedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 20)
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "reason is mandatory (min 20 characters).", ErrorId = -1 });
        }

        return _adminMembersRepository.HoldMemberAsync(clientId, id, request.Reason, changedBy, cancellationToken);
    }

    public Task<ResponseModel<bool>> RejectMemberAsync(int clientId, int id, RejectMemberRequest request, int changedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Feedback))
        {
            return Task.FromResult(new ResponseModel<bool> { ErrorMessage = "feedback is mandatory.", ErrorId = -1 });
        }

        return _adminMembersRepository.RejectMemberAsync(clientId, id, request.Feedback, changedBy, cancellationToken);
    }
}

