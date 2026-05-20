using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class AdminFirmsRepository : IAdminFirmsRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<AdminFirmsRepository> _logger;

    public AdminFirmsRepository(ISqlConnectionFactory connection, ILogger<AdminFirmsRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    private sealed class FirmRow : FirmListItemResponse
    {
        public int Total { get; init; }
    }

    public async Task<ResponseModel<PagedResponse<FirmListItemResponse>>> GetFirmsAsync(int clientId, int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_GetFirms",
                new { ClientId = clientId, Page = page, PageSize = pageSize, Search = search },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = (await connection.QueryAsync<FirmRow>(cmd)).ToList();
            var total = rows.FirstOrDefault()?.Total ?? 0;

            return new ResponseModel<PagedResponse<FirmListItemResponse>>
            {
                Data = new PagedResponse<FirmListItemResponse>
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    Records = rows.Select(r => new FirmListItemResponse
                    {
                        FirmId = r.FirmId,
                        Name = r.Name,
                        GstNo = r.GstNo,
                        City = r.City,
                        CompanyTypeId = r.CompanyTypeId,
                        CompanyTypeName = r.CompanyTypeName,
                        CompanyCode = r.CompanyCode,
                        RegNo = r.RegNo,
                        IsActive = r.IsActive,
                        CreatedDate = r.CreatedDate
                    }).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching firms.");
            return new ResponseModel<PagedResponse<FirmListItemResponse>> { ErrorMessage = "Unable to fetch firms.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<FirmDetailResponse>> GetFirmDetailAsync(int clientId, int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_GetFirmDetail",
                new { ClientId = clientId, FirmId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var row = await connection.QueryFirstOrDefaultAsync<FirmDetailResponse>(cmd);
            return row == null
                ? new ResponseModel<FirmDetailResponse> { ErrorMessage = "Firm not found.", ErrorId = -1 }
                : new ResponseModel<FirmDetailResponse> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching firm detail.");
            return new ResponseModel<FirmDetailResponse> { ErrorMessage = "Unable to fetch firm detail.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> CreateFirmAsync(int clientId, CreateFirmRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            DateTime? est = request.DateOfEstablishment.HasValue
                ? request.DateOfEstablishment.Value.Date
                : null;

            var cmd = new CommandDefinition(
                "usp_Admin_CreateFirm",
                new
                {
                    ClientId = clientId,
                    Name = request.Name,
                    CompanyTypeId = request.CompanyTypeId,
                    GstNo = request.GstNo,
                    CompanyCode = request.CompanyCode,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PinCode = request.PinCode,
                    DateOfEstablishment = est,
                    RegNo = request.RegNo,
                    TelephoneNo = request.TelephoneNo,
                    Mobile = request.Mobile,
                    Website = request.Website,
                    Products = request.Products,
                    CreatedBy = createdBy
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var newId = await connection.QuerySingleAsync<int>(cmd);
            return new ResponseModel<int> { Data = newId };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<int> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating firm.");
            return new ResponseModel<int> { ErrorMessage = "Unable to create firm.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateFirmAsync(int clientId, int id, UpdateFirmRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            DateTime? est = request.DateOfEstablishment.HasValue
                ? request.DateOfEstablishment.Value.Date
                : null;

            var cmd = new CommandDefinition(
                "usp_Admin_UpdateFirm",
                new
                {
                    ClientId = clientId,
                    FirmId = id,
                    Name = request.Name,
                    CompanyTypeId = request.CompanyTypeId,
                    GstNo = request.GstNo,
                    CompanyCode = request.CompanyCode,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PinCode = request.PinCode,
                    DateOfEstablishment = est,
                    RegNo = request.RegNo,
                    TelephoneNo = request.TelephoneNo,
                    Mobile = request.Mobile,
                    Website = request.Website,
                    Products = request.Products,
                    IsActive = request.IsActive,
                    ModifiedBy = modifiedBy
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating firm.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update firm.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> SoftDeleteFirmAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_Admin_SoftDeleteFirm",
                new { ClientId = clientId, FirmId = id, ModifiedBy = modifiedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting firm.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to delete firm.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> CreateFirmDocumentAsync(int clientId, int id, string documentType, string blobUrl, int uploadedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_FirmDocument_Create",
                new { ClientId = clientId, FirmId = id, DocumentType = documentType, BlobUrl = blobUrl, UploadedBy = uploadedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating firm document.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to upload firm document.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> LinkMemberAsync(int clientId, int id, int memberId, string roleInFirm, int linkedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_FirmMember_Link",
                new { ClientId = clientId, FirmId = id, MemberId = memberId, RoleInFirm = roleInFirm, LinkedBy = linkedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while linking firm member.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to link member.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UnlinkMemberAsync(int clientId, int id, int memberId, int unlinkedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connection.GetConnection();
            var cmd = new CommandDefinition(
                "usp_FirmMember_Unlink",
                new { ClientId = clientId, FirmId = id, MemberId = memberId, UnlinkedBy = unlinkedBy },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affected = await connection.ExecuteAsync(cmd);
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return new ResponseModel<bool> { ErrorMessage = ex.Message, ErrorId = -1 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while unlinking firm member.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to unlink member.", ErrorId = -1 };
        }
    }
}
