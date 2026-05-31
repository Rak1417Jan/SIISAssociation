namespace MVEA.Model.DTOs.Platform;

// ── Plans & Payments ──

public sealed class MembershipPlanDto
{
    public int PlanId { get; init; }
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int? Duration { get; init; }
    public decimal Price { get; init; }
    public decimal GstPercent { get; init; }
    public decimal GstAmount { get; init; }
    public decimal PlatformFee { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<string> Features { get; init; } = Array.Empty<string>();
}

public sealed class CreatePaymentOrderRequest
{
    public int PlanId { get; init; }
    public int? MemberId { get; init; }
}

public sealed class CreatePaymentOrderResponse
{
    public string OrderId { get; init; } = string.Empty;
    public int Amount { get; init; }
    public string Currency { get; init; } = "INR";
    public string KeyId { get; init; } = string.Empty;
    public PaymentPrefillDto Prefill { get; init; } = new();
}

public sealed class PaymentPrefillDto
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Contact { get; init; } = string.Empty;
}

public sealed class VerifyPaymentRequest
{
    public string RazorpayOrderId { get; init; } = string.Empty;
    public string RazorpayPaymentId { get; init; } = string.Empty;
    public string RazorpaySignature { get; init; } = string.Empty;
    public int PlanId { get; init; }
}

public sealed class VerifyPaymentResponse
{
    public bool Success { get; init; }
    public int PaymentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string ReceiptUrl { get; init; } = string.Empty;
    public string ReceiptNo { get; init; } = string.Empty;
}

public sealed class PaymentHistoryItemDto
{
    public int PaymentId { get; init; }
    public string ReceiptNo { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string PaymentMethod { get; init; } = string.Empty;
    public DateTime? PaidDate { get; init; }
    public string PlanName { get; init; } = string.Empty;
}

public sealed class PaymentSummaryDto
{
    public decimal TotalPaid { get; init; }
    public int PaymentCount { get; init; }
    public DateTime? LastPaymentDate { get; init; }
    public string? CurrentPlanName { get; init; }
}

public sealed class RenewalPaymentRequest
{
    public int PlanId { get; init; }
}

public sealed class RefundPaymentRequest
{
    public int PaymentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public sealed class RefundPaymentResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}

// ── Registration ──

public sealed class StartRegistrationRequest
{
    public string CompanyName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
}

public sealed class StartRegistrationResponse
{
    public int ApplicationId { get; init; }
    public int CurrentStep { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class RegistrationStepRequest
{
    public int Step { get; init; }
    public string DraftDataJson { get; init; } = string.Empty;
}

public sealed class SubmitRegistrationRequest
{
    public string? FinalDataJson { get; init; }
}

public sealed class RegistrationStatusResponse
{
    public int ApplicationId { get; init; }
    public int CurrentStep { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? DraftDataJson { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public DateTime? SubmittedDate { get; init; }
}

// ── Documents ──

public sealed class DocumentUploadResponse
{
    public int DocumentId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string BlobUrl { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class DocumentListItemDto
{
    public int DocumentId { get; init; }
    public int ApplicationId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string BlobUrl { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool? AiValid { get; init; }
    public decimal? AiConfidence { get; init; }
    public string? AiReason { get; init; }
    public DateTime UploadedAt { get; init; }
}

public sealed class DocumentAiVerifyResponse
{
    public int DocumentId { get; init; }
    public bool AiValid { get; init; }
    public decimal AiConfidence { get; init; }
    public string AiReason { get; init; } = string.Empty;
}

public sealed class VerifyDocumentRequest
{
    public string Status { get; init; } = string.Empty;
    public string? Remarks { get; init; }
}

// ── Digital ID ──

public sealed class DigitalIdResponse
{
    public int MemberId { get; init; }
    public string MembershipId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public string? QrCodeUrl { get; init; }
    public string? QrValue { get; init; }
    public string? Designation { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsGenerated { get; init; }
}

public sealed class DigitalIdVerifyResponse
{
    public bool Valid { get; init; }
    public string MembershipId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? ValidUntil { get; init; }
}

public sealed class ShareDigitalIdRequest
{
    public string Channel { get; init; } = "WhatsApp";
    public string? Recipient { get; init; }
}

// ── Audit ──

public sealed class AuditLogListItemDto
{
    public int AuditId { get; init; }
    public int? UserId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public int? EntityId { get; init; }
    public string? TargetLabel { get; init; }
    public string? StaffRole { get; init; }
    public string? IpAddress { get; init; }
    public DateTime ActionDate { get; init; }
}

public sealed class AuditLogFilterRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int? StaffId { get; init; }
    public string? ActionType { get; init; }
    public string? EntityType { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public sealed class WriteAuditLogRequest
{
    public int UserId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string? TargetLabel { get; init; }
    public string? StaffRole { get; init; }
    public string? IpAddress { get; init; }
    public string? ChangesJson { get; init; }
}

// ── Settings ──

public sealed class ClientSettingsDto
{
    public int ClientId { get; init; }
    public string AssociationName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public decimal? YearlyFee { get; init; }
    public decimal? LifetimeFee { get; init; }
    public decimal? GstPercent { get; init; }
    public decimal? PlatformFeeFlat { get; init; }
    public string? GstNumber { get; init; }
    public string? Address { get; init; }
    public string? SupportPhone { get; init; }
    public string? SupportEmail { get; init; }
    public bool WhatsappEnabled { get; init; }
    public bool SmsEnabled { get; init; }
    public bool AutoApproval { get; init; }
    public int RenewalReminderDays { get; init; }
}

public sealed class UpdateClientSettingsRequest
{
    public string? AssociationName { get; init; }
    public string? PrimaryColor { get; init; }
    public decimal? YearlyFee { get; init; }
    public decimal? LifetimeFee { get; init; }
    public decimal? GstPercent { get; init; }
    public decimal? PlatformFeeFlat { get; init; }
    public string? GstNumber { get; init; }
    public string? Address { get; init; }
    public string? SupportPhone { get; init; }
    public string? SupportEmail { get; init; }
    public bool? WhatsappEnabled { get; init; }
    public bool? SmsEnabled { get; init; }
    public bool? AutoApproval { get; init; }
    public int? RenewalReminderDays { get; init; }
}

// ── Broadcasts v1 ──

public sealed class ScheduleBroadcastRequest
{
    public DateTime ScheduledAt { get; init; }
}

public sealed class BroadcastStatsResponse
{
    public int BroadcastId { get; init; }
    public int RecipientCount { get; init; }
    public int DeliveredCount { get; init; }
    public int FailedCount { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? ScheduledAt { get; init; }
}

// ── Engagement ──

public sealed class DirectoryMemberDto
{
    public int MemberId { get; init; }
    public string MembershipId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public string? City { get; init; }
    public string? CompanyName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public sealed class IndustryDto
{
    public int CompanyTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public class EventListItemDto
{
    public int EventId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? EventType { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime EventDate { get; init; }
    public string? EventTime { get; init; }
    public string? Venue { get; init; }
    public int TotalSeats { get; init; }
    public int BookedSeats { get; init; }
    public decimal TicketPrice { get; init; }
    public bool IsFree { get; init; }
    public bool IsOnline { get; init; }
}

public sealed class EventDetailDto : EventListItemDto
{
    public string? Description { get; init; }
    public string? MeetLink { get; init; }
    public DateTime? RsvpDeadline { get; init; }
    public string? MyRsvp { get; init; }
}

public class CreateEventRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? EventType { get; init; }
    public DateTime EventDate { get; init; }
    public string? EventTime { get; init; }
    public string? Venue { get; init; }
    public int TotalSeats { get; init; }
    public decimal TicketPrice { get; init; }
    public bool IsFree { get; init; } = true;
    public bool IsOnline { get; init; }
    public string? MeetLink { get; init; }
    public DateTime? RsvpDeadline { get; init; }
}

public sealed class UpdateEventRequest : CreateEventRequest
{
    public string? Status { get; init; }
}

public sealed class EventRsvpRequest
{
    public string Response { get; init; } = "YES";
}

public sealed class ReferralCodeResponse
{
    public string ReferralCode { get; init; } = string.Empty;
}

public sealed class TrackReferralRequest
{
    public string ReferralCode { get; init; } = string.Empty;
    public string RefereeName { get; init; } = string.Empty;
    public string RefereePhone { get; init; } = string.Empty;
    public string? RefereeFirm { get; init; }
}

public sealed class ReferralStatsDto
{
    public int TotalReferrals { get; init; }
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
}

public sealed class ReferralListItemDto
{
    public int ReferralId { get; init; }
    public string ReferralCode { get; init; } = string.Empty;
    public string RefereeName { get; init; } = string.Empty;
    public string RefereePhone { get; init; } = string.Empty;
    public string? RefereeFirm { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime AppliedAt { get; init; }
}

public sealed class SubmitGrievanceRequest
{
    public string Subject { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Priority { get; init; } = "MEDIUM";
}

public sealed class GrievanceListItemDto
{
    public int GrievanceId { get; init; }
    public string TicketNo { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime SubmittedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? AdminResponse { get; init; }
}

public sealed class UpdateGrievanceRequest
{
    public string Status { get; init; } = string.Empty;
    public string? AdminResponse { get; init; }
}
