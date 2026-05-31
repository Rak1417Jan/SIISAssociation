using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.IRepository;

public interface IPlatformRepository
{
    // Plans & Payments
    Task<ResponseModel<IReadOnlyList<MembershipPlanDto>>> GetPlansAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<MembershipPlanDto?>> GetPlanByIdAsync(int clientId, int planId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> InsertPaymentAsync(int memberId, int planId, decimal amount, string orderId, string status, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdatePaymentVerifiedAsync(string orderId, string paymentId, string receiptNo, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(int clientId, int memberId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<PaymentSummaryDto>> GetPaymentSummaryAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdatePaymentRefundedAsync(int clientId, int paymentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<(string Name, string Email, string Mobile)?>> GetMemberContactAsync(int memberId, CancellationToken cancellationToken = default);

    // Registration
    Task<ResponseModel<int>> StartApplicationAsync(int clientId, StartRegistrationRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateApplicationStepAsync(int clientId, int applicationId, RegistrationStepRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> SubmitApplicationAsync(int clientId, int applicationId, SubmitRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<RegistrationStatusResponse>> GetApplicationStatusAsync(int clientId, int applicationId, CancellationToken cancellationToken = default);

    // Documents
    Task<ResponseModel<int>> InsertDocumentAsync(int clientId, int applicationId, string documentType, string blobUrl, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DocumentListItemDto>>> GetDocumentsAsync(int clientId, int applicationId, CancellationToken cancellationToken = default);
    Task<ResponseModel<DocumentListItemDto?>> GetDocumentByIdAsync(int clientId, int documentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateDocumentAiResultAsync(int documentId, bool aiValid, decimal confidence, string reason, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> VerifyDocumentAsync(int clientId, int documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default);

    // Digital ID
    Task<ResponseModel<DigitalIdResponse?>> GetDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpsertDigitalIdAsync(int clientId, int memberId, DigitalIdResponse data, CancellationToken cancellationToken = default);
    Task<ResponseModel<DigitalIdVerifyResponse?>> VerifyDigitalIdAsync(string membershipId, CancellationToken cancellationToken = default);

    // Audit
    Task<ResponseModel<PagedResponse<AuditLogListItemDto>>> GetAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> WriteAuditLogAsync(WriteAuditLogRequest request, CancellationToken cancellationToken = default);

    // Settings
    Task<ResponseModel<ClientSettingsDto>> GetClientSettingsAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpsertClientSettingsAsync(int clientId, UpdateClientSettingsRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateClientLogoAsync(int clientId, string logoUrl, CancellationToken cancellationToken = default);

    // Engagement - Directory
    Task<ResponseModel<PagedResponse<DirectoryMemberDto>>> GetDirectoryMembersAsync(int clientId, int page, int pageSize, string? search, bool includeContact, CancellationToken cancellationToken = default);
    Task<ResponseModel<DirectoryMemberDto?>> GetDirectoryMemberAsync(int clientId, int memberId, bool includeContact, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<IndustryDto>>> GetIndustriesAsync(CancellationToken cancellationToken = default);

    // Events
    Task<ResponseModel<IReadOnlyList<EventListItemDto>>> GetEventsAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<EventDetailDto?>> GetEventByIdAsync(int clientId, int eventId, int? memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateEventAsync(int clientId, CreateEventRequest request, int createdBy, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateEventAsync(int clientId, int eventId, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> DeleteEventAsync(int clientId, int eventId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpsertEventRsvpAsync(int eventId, int memberId, string response, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> CancelEventRsvpAsync(int eventId, int memberId, CancellationToken cancellationToken = default);

    // Referrals
    Task<ResponseModel<string>> GetOrCreateReferralCodeAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> TrackReferralAsync(int clientId, TrackReferralRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<ReferralStatsDto>> GetReferralStatsAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<ReferralListItemDto>>> GetReferralsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);

    // Grievances
    Task<ResponseModel<int>> SubmitGrievanceAsync(int clientId, int memberId, SubmitGrievanceRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<GrievanceListItemDto>>> GetMyGrievancesAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<GrievanceListItemDto>>> GetGrievancesAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateGrievanceAsync(int clientId, int grievanceId, UpdateGrievanceRequest request, CancellationToken cancellationToken = default);
}
