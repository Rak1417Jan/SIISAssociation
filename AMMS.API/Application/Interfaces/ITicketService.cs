using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int voterId, CancellationToken cancellationToken = default);
    Task<TicketDetailResponse> GetTicketByIdAsync(int id, int? userId = null, bool isVoter = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<TicketResponse>> GetTicketsByVoterAsync(int voterId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTicketStatusAsync(int id, UpdateTicketStatusRequest request, int userId, CancellationToken cancellationToken = default);
    Task<TicketReportResponse> GetTicketReportAsync(int? assemblyId = null, int? mlaId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
