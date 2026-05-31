using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using Microsoft.AspNetCore.Http;

namespace MVEA.Services.IService;

public interface IPlatformService
{
    // Plans & Payments
    Task<ResponseModel<IReadOnlyList<MembershipPlanDto>>> GetPlansAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<CreatePaymentOrderResponse>> CreatePaymentOrderAsync(int clientId, int memberId, CreatePaymentOrderRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<VerifyPaymentResponse>> VerifyPaymentAsync(int clientId, int memberId, VerifyPaymentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(int clientId, int memberId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<PaymentSummaryDto>> GetPaymentSummaryAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<CreatePaymentOrderResponse>> CreateRenewalOrderAsync(int clientId, int memberId, RenewalPaymentRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<RefundPaymentResponse>> RefundPaymentAsync(int clientId, RefundPaymentRequest request, CancellationToken cancellationToken = default);

    // Registration
    Task<ResponseModel<StartRegistrationResponse>> StartRegistrationAsync(int clientId, StartRegistrationRequest request, int userId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> SaveRegistrationStepAsync(int clientId, int applicationId, RegistrationStepRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> SubmitRegistrationAsync(int clientId, int applicationId, SubmitRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<RegistrationStatusResponse>> GetRegistrationStatusAsync(int clientId, int applicationId, CancellationToken cancellationToken = default);

    // Documents
    Task<ResponseModel<DocumentUploadResponse>> UploadDocumentAsync(int clientId, int applicationId, string documentType, IFormFile file, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<DocumentListItemDto>>> GetDocumentsAsync(int clientId, int applicationId, CancellationToken cancellationToken = default);
    Task<ResponseModel<DocumentAiVerifyResponse>> AiVerifyDocumentAsync(int clientId, int documentId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> VerifyDocumentAsync(int clientId, int documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default);

    // Digital ID
    Task<ResponseModel<DigitalIdResponse>> GetDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<DigitalIdResponse>> GenerateDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<byte[]>> DownloadDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<DigitalIdVerifyResponse>> VerifyDigitalIdPublicAsync(string membershipId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> ShareDigitalIdAsync(int clientId, int memberId, ShareDigitalIdRequest request, CancellationToken cancellationToken = default);

    // Audit
    Task<ResponseModel<PagedResponse<AuditLogListItemDto>>> GetAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task<ResponseModel<byte[]>> ExportAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default);
    Task WriteAuditLogAsync(WriteAuditLogRequest request, CancellationToken cancellationToken = default);

    // Settings
    Task<ResponseModel<ClientSettingsDto>> GetSettingsAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateSettingsAsync(int clientId, UpdateClientSettingsRequest request, int userId, CancellationToken cancellationToken = default);
    Task<ResponseModel<string>> UploadLogoAsync(int clientId, IFormFile file, CancellationToken cancellationToken = default);

    // Engagement
    Task<ResponseModel<PagedResponse<DirectoryMemberDto>>> GetDirectoryMembersAsync(int clientId, int page, int pageSize, string? search, bool includeContact, CancellationToken cancellationToken = default);
    Task<ResponseModel<DirectoryMemberDto>> GetDirectoryMemberAsync(int clientId, int memberId, bool includeContact, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<IndustryDto>>> GetIndustriesAsync(CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<EventListItemDto>>> GetEventsAsync(int clientId, CancellationToken cancellationToken = default);
    Task<ResponseModel<EventDetailDto>> GetEventByIdAsync(int clientId, int eventId, int? memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> CreateEventAsync(int clientId, CreateEventRequest request, int userId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateEventAsync(int clientId, int eventId, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> DeleteEventAsync(int clientId, int eventId, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> RsvpEventAsync(int eventId, int memberId, EventRsvpRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> CancelEventRsvpAsync(int eventId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<ReferralCodeResponse>> GetMyReferralCodeAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> TrackReferralAsync(int clientId, TrackReferralRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<ReferralStatsDto>> GetReferralStatsAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<ReferralListItemDto>>> GetReferralsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<int>> SubmitGrievanceAsync(int clientId, int memberId, SubmitGrievanceRequest request, CancellationToken cancellationToken = default);
    Task<ResponseModel<IReadOnlyList<GrievanceListItemDto>>> GetMyGrievancesAsync(int clientId, int memberId, CancellationToken cancellationToken = default);
    Task<ResponseModel<PagedResponse<GrievanceListItemDto>>> GetGrievancesAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResponseModel<bool>> UpdateGrievanceAsync(int clientId, int grievanceId, UpdateGrievanceRequest request, CancellationToken cancellationToken = default);
}
