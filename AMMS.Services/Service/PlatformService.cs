using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;
using MVEA.Services.Messaging;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MVEA.Services.Service;

public sealed class PlatformService : IPlatformService
{
    private readonly IPlatformRepository _platformRepository;
    private readonly IOutboundNotifier _notifier;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PlatformService> _logger;

    public PlatformService(
        IPlatformRepository platformRepository,
        IOutboundNotifier notifier,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<PlatformService> logger)
    {
        _platformRepository = platformRepository;
        _notifier = notifier;
        _configuration = configuration;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ── Plans & Payments ──

    public Task<ResponseModel<IReadOnlyList<MembershipPlanDto>>> GetPlansAsync(int clientId, CancellationToken cancellationToken = default)
        => _platformRepository.GetPlansAsync(clientId, cancellationToken);

    public async Task<ResponseModel<CreatePaymentOrderResponse>> CreatePaymentOrderAsync(int clientId, int memberId, CreatePaymentOrderRequest request, CancellationToken cancellationToken = default)
    {
        ResponseModel<MembershipPlanDto?> planResult = await _platformRepository.GetPlanByIdAsync(clientId, request.PlanId, cancellationToken);
        if (!planResult.Success || planResult.Data == null)
        {
            return new ResponseModel<CreatePaymentOrderResponse> { ErrorMessage = planResult.ErrorMessage ?? "Plan not found.", ErrorId = -1 };
        }

        int effectiveMemberId = request.MemberId ?? memberId;
        if (effectiveMemberId <= 0)
        {
            return new ResponseModel<CreatePaymentOrderResponse> { ErrorMessage = "Member ID is required.", ErrorId = -1 };
        }

        MembershipPlanDto plan = planResult.Data;
        int amountPaise = (int)(plan.TotalAmount * 100);
        string keyId = _configuration["RAZORPAY_KEY_ID"] ?? string.Empty;
        string keySecret = _configuration["RAZORPAY_KEY_SECRET"] ?? string.Empty;
        string orderId;

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            orderId = $"order_stub_{Guid.NewGuid():N}";
            _logger.LogWarning("Razorpay credentials missing; using stub order {OrderId}.", orderId);
        }
        else
        {
            HttpClient client = _httpClientFactory.CreateClient("Razorpay");
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var payload = new { amount = amountPaise, currency = "INR", receipt = $"rcpt_{effectiveMemberId}_{DateTime.UtcNow:yyyyMMddHHmmss}" };
            HttpResponseMessage response = await client.PostAsync(
                "orders",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay order creation failed: {Status}", response.StatusCode);
                return new ResponseModel<CreatePaymentOrderResponse> { ErrorMessage = "Payment service unavailable.", ErrorId = -1 };
            }

            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            orderId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;
        }

        ResponseModel<int> paymentRecord = await _platformRepository.InsertPaymentAsync(
            effectiveMemberId, request.PlanId, plan.TotalAmount, orderId, "PENDING", effectiveMemberId, cancellationToken);

        if (!paymentRecord.Success)
        {
            return new ResponseModel<CreatePaymentOrderResponse> { ErrorMessage = paymentRecord.ErrorMessage, ErrorId = paymentRecord.ErrorId };
        }

        ResponseModel<(string Name, string Email, string Mobile)?> contact = await _platformRepository.GetMemberContactAsync(effectiveMemberId, cancellationToken);

        return new ResponseModel<CreatePaymentOrderResponse>
        {
            Data = new CreatePaymentOrderResponse
            {
                OrderId = orderId,
                Amount = amountPaise,
                Currency = "INR",
                KeyId = string.IsNullOrWhiteSpace(keyId) ? "rzp_test_stub" : keyId,
                Prefill = new PaymentPrefillDto
                {
                    Name = contact.Data?.Name ?? string.Empty,
                    Email = contact.Data?.Email ?? string.Empty,
                    Contact = contact.Data?.Mobile ?? string.Empty
                }
            }
        };
    }

    public async Task<ResponseModel<VerifyPaymentResponse>> VerifyPaymentAsync(int clientId, int memberId, VerifyPaymentRequest request, CancellationToken cancellationToken = default)
    {
        string keySecret = _configuration["RAZORPAY_KEY_SECRET"] ?? string.Empty;
        bool signatureValid;

        if (string.IsNullOrWhiteSpace(keySecret))
        {
            signatureValid = request.RazorpayOrderId.StartsWith("order_stub_", StringComparison.Ordinal);
            _logger.LogWarning("Razorpay secret missing; stub verification for {OrderId}.", request.RazorpayOrderId);
        }
        else
        {
            string payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
            using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string expected = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            signatureValid = string.Equals(expected, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase);
        }

        if (!signatureValid)
        {
            return new ResponseModel<VerifyPaymentResponse> { ErrorMessage = "Invalid payment signature.", ErrorId = -1 };
        }

        string receiptNo = $"RCP-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(10000, 99999)}";
        ResponseModel<bool> updated = await _platformRepository.UpdatePaymentVerifiedAsync(
            request.RazorpayOrderId, request.RazorpayPaymentId, receiptNo, cancellationToken);

        if (!updated.Success || !updated.Data)
        {
            return new ResponseModel<VerifyPaymentResponse> { ErrorMessage = updated.ErrorMessage ?? "Payment record not found.", ErrorId = -1 };
        }

        await WriteAuditLogAsync(new WriteAuditLogRequest
        {
            UserId = memberId,
            Action = "PAYMENT_VERIFIED",
            EntityName = "PAYMENTS",
            EntityId = 0,
            TargetLabel = receiptNo
        }, cancellationToken);

        return new ResponseModel<VerifyPaymentResponse>
        {
            Data = new VerifyPaymentResponse
            {
                Success = true,
                PaymentId = 0,
                Status = "SUCCESS",
                ReceiptUrl = $"/receipts/{receiptNo}",
                ReceiptNo = receiptNo
            }
        };
    }

    public Task<ResponseModel<PagedResponse<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(int clientId, int memberId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = NormalizePage(page);
        pageSize = NormalizePageSize(pageSize);
        return _platformRepository.GetPaymentHistoryAsync(clientId, memberId, page, pageSize, cancellationToken);
    }

    public Task<ResponseModel<PaymentSummaryDto>> GetPaymentSummaryAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
        => _platformRepository.GetPaymentSummaryAsync(clientId, memberId, cancellationToken);

    public Task<ResponseModel<CreatePaymentOrderResponse>> CreateRenewalOrderAsync(int clientId, int memberId, RenewalPaymentRequest request, CancellationToken cancellationToken = default)
        => CreatePaymentOrderAsync(clientId, memberId, new CreatePaymentOrderRequest { PlanId = request.PlanId, MemberId = memberId }, cancellationToken);

    public async Task<ResponseModel<RefundPaymentResponse>> RefundPaymentAsync(int clientId, RefundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        ResponseModel<bool> result = await _platformRepository.UpdatePaymentRefundedAsync(clientId, request.PaymentId, cancellationToken);
        return new ResponseModel<RefundPaymentResponse>
        {
            Data = new RefundPaymentResponse
            {
                Success = result.Success && result.Data,
                Message = result.Success && result.Data ? "Refund processed." : result.ErrorMessage ?? "Refund failed."
            },
            ErrorMessage = result.Success ? string.Empty : result.ErrorMessage,
            ErrorId = result.ErrorId
        };
    }

    // ── Registration ──

    public async Task<ResponseModel<StartRegistrationResponse>> StartRegistrationAsync(int clientId, StartRegistrationRequest request, int userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            return new ResponseModel<StartRegistrationResponse> { ErrorMessage = "Company name is required.", ErrorId = -1 };
        }

        ResponseModel<int> created = await _platformRepository.StartApplicationAsync(clientId, request, userId, cancellationToken);
        if (!created.Success)
        {
            return new ResponseModel<StartRegistrationResponse> { ErrorMessage = created.ErrorMessage, ErrorId = created.ErrorId };
        }

        return new ResponseModel<StartRegistrationResponse>
        {
            Data = new StartRegistrationResponse { ApplicationId = created.Data, CurrentStep = 1, Status = "DRAFT" }
        };
    }

    public Task<ResponseModel<bool>> SaveRegistrationStepAsync(int clientId, int applicationId, RegistrationStepRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.UpdateApplicationStepAsync(clientId, applicationId, request, cancellationToken);

    public Task<ResponseModel<bool>> SubmitRegistrationAsync(int clientId, int applicationId, SubmitRegistrationRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.SubmitApplicationAsync(clientId, applicationId, request, cancellationToken);

    public Task<ResponseModel<RegistrationStatusResponse>> GetRegistrationStatusAsync(int clientId, int applicationId, CancellationToken cancellationToken = default)
        => _platformRepository.GetApplicationStatusAsync(clientId, applicationId, cancellationToken);

    // ── Documents ──

    public async Task<ResponseModel<DocumentUploadResponse>> UploadDocumentAsync(int clientId, int applicationId, string documentType, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return new ResponseModel<DocumentUploadResponse> { ErrorMessage = "File is required.", ErrorId = -1 };
        }

        string uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", clientId.ToString(), applicationId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        string safeName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        string fullPath = Path.Combine(uploadsRoot, safeName);

        await using (FileStream stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        string blobUrl = $"/uploads/{clientId}/{applicationId}/{safeName}";
        ResponseModel<int> inserted = await _platformRepository.InsertDocumentAsync(clientId, applicationId, documentType, blobUrl, cancellationToken);

        if (!inserted.Success)
        {
            return new ResponseModel<DocumentUploadResponse> { ErrorMessage = inserted.ErrorMessage, ErrorId = inserted.ErrorId };
        }

        return new ResponseModel<DocumentUploadResponse>
        {
            Data = new DocumentUploadResponse
            {
                DocumentId = inserted.Data,
                DocumentType = documentType,
                BlobUrl = blobUrl,
                Status = "PROCESSING"
            }
        };
    }

    public Task<ResponseModel<IReadOnlyList<DocumentListItemDto>>> GetDocumentsAsync(int clientId, int applicationId, CancellationToken cancellationToken = default)
        => _platformRepository.GetDocumentsAsync(clientId, applicationId, cancellationToken);

    public async Task<ResponseModel<DocumentAiVerifyResponse>> AiVerifyDocumentAsync(int clientId, int documentId, CancellationToken cancellationToken = default)
    {
        ResponseModel<DocumentListItemDto?> doc = await _platformRepository.GetDocumentByIdAsync(clientId, documentId, cancellationToken);
        if (!doc.Success || doc.Data == null)
        {
            return new ResponseModel<DocumentAiVerifyResponse> { ErrorMessage = doc.ErrorMessage ?? "Document not found.", ErrorId = -1 };
        }

        bool aiValid = true;
        decimal confidence = 0.95m;
        string reason = "Stub AI verification passed.";
        await _platformRepository.UpdateDocumentAiResultAsync(documentId, aiValid, confidence, reason, cancellationToken);

        return new ResponseModel<DocumentAiVerifyResponse>
        {
            Data = new DocumentAiVerifyResponse { DocumentId = documentId, AiValid = aiValid, AiConfidence = confidence, AiReason = reason }
        };
    }

    public Task<ResponseModel<bool>> VerifyDocumentAsync(int clientId, int documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.VerifyDocumentAsync(clientId, documentId, request, cancellationToken);

    // ── Digital ID ──

    public Task<ResponseModel<DigitalIdResponse>> GetDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
        => WrapNullable(_platformRepository.GetDigitalIdAsync(clientId, memberId, cancellationToken));

    public async Task<ResponseModel<DigitalIdResponse>> GenerateDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        ResponseModel<DigitalIdResponse?> existing = await _platformRepository.GetDigitalIdAsync(clientId, memberId, cancellationToken);
        if (!existing.Success)
        {
            return new ResponseModel<DigitalIdResponse> { ErrorMessage = existing.ErrorMessage, ErrorId = existing.ErrorId };
        }

        DigitalIdResponse source = existing.Data ?? new DigitalIdResponse { MemberId = memberId };
        DigitalIdResponse data = new DigitalIdResponse
        {
            MemberId = memberId,
            OwnerName = source.OwnerName,
            PhotoUrl = source.PhotoUrl,
            Designation = source.Designation,
            MembershipId = string.IsNullOrWhiteSpace(source.MembershipId) ? $"AMMS-{DateTime.UtcNow:yyyy}-{memberId:D4}" : source.MembershipId,
            QrValue = $"AMMS:{memberId}:{Guid.NewGuid():N}",
            QrCodeUrl = $"/uploads/qr/{memberId}.png",
            ValidFrom = DateTime.UtcNow.Date,
            ValidUntil = DateTime.UtcNow.Date.AddYears(1),
            Status = "ACTIVE",
            IsGenerated = true
        };

        ResponseModel<bool> saved = await _platformRepository.UpsertDigitalIdAsync(clientId, memberId, data, cancellationToken);
        if (!saved.Success)
        {
            return new ResponseModel<DigitalIdResponse> { ErrorMessage = saved.ErrorMessage, ErrorId = saved.ErrorId };
        }

        return new ResponseModel<DigitalIdResponse> { Data = data };
    }

    public async Task<ResponseModel<byte[]>> DownloadDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        ResponseModel<DigitalIdResponse?> id = await _platformRepository.GetDigitalIdAsync(clientId, memberId, cancellationToken);
        if (!id.Success || id.Data == null)
        {
            return new ResponseModel<byte[]> { ErrorMessage = id.ErrorMessage ?? "Digital ID not found.", ErrorId = -1 };
        }

        string content = $"Digital ID: {id.Data.MembershipId}\nName: {id.Data.OwnerName}\nValid Until: {id.Data.ValidUntil:yyyy-MM-dd}";
        return new ResponseModel<byte[]> { Data = Encoding.UTF8.GetBytes(content) };
    }

    public Task<ResponseModel<DigitalIdVerifyResponse>> VerifyDigitalIdPublicAsync(string membershipId, CancellationToken cancellationToken = default)
        => WrapNullable(_platformRepository.VerifyDigitalIdAsync(membershipId, cancellationToken));

    public async Task<ResponseModel<bool>> ShareDigitalIdAsync(int clientId, int memberId, ShareDigitalIdRequest request, CancellationToken cancellationToken = default)
    {
        ResponseModel<DigitalIdResponse?> id = await _platformRepository.GetDigitalIdAsync(clientId, memberId, cancellationToken);
        if (!id.Success || id.Data == null)
        {
            return new ResponseModel<bool> { ErrorMessage = "Digital ID not found.", ErrorId = -1 };
        }

        string message = $"Your AMMS Digital ID: {id.Data.MembershipId}";
        string recipient = request.Recipient ?? string.Empty;

        if (request.Channel.Equals("SMS", StringComparison.OrdinalIgnoreCase))
        {
            await _notifier.SendSmsAsync(recipient, message, cancellationToken);
        }
        else
        {
            await _notifier.SendWhatsAppAsync(recipient, message, cancellationToken);
        }

        return new ResponseModel<bool> { Data = true };
    }

    // ── Audit ──

    public Task<ResponseModel<PagedResponse<AuditLogListItemDto>>> GetAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default)
    {
        AuditLogFilterRequest normalized = new AuditLogFilterRequest
        {
            Page = NormalizePage(filter.Page),
            PageSize = NormalizePageSize(filter.PageSize),
            StaffId = filter.StaffId,
            ActionType = filter.ActionType,
            EntityType = filter.EntityType,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo
        };
        return _platformRepository.GetAuditLogsAsync(clientId, normalized, cancellationToken);
    }

    public async Task<ResponseModel<byte[]>> ExportAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default)
    {
        AuditLogFilterRequest exportFilter = new AuditLogFilterRequest
        {
            Page = 1,
            PageSize = 10000,
            StaffId = filter.StaffId,
            ActionType = filter.ActionType,
            EntityType = filter.EntityType,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo
        };
        ResponseModel<PagedResponse<AuditLogListItemDto>> logs = await _platformRepository.GetAuditLogsAsync(clientId, exportFilter, cancellationToken);
        if (!logs.Success || logs.Data == null)
        {
            return new ResponseModel<byte[]> { ErrorMessage = logs.ErrorMessage, ErrorId = logs.ErrorId };
        }

        StringBuilder csv = new StringBuilder("AuditId,UserId,Action,EntityName,EntityId,ActionDate\n");
        foreach (AuditLogListItemDto row in logs.Data.Records)
        {
            csv.AppendLine($"{row.AuditId},{row.UserId},{row.Action},{row.EntityName},{row.EntityId},{row.ActionDate:O}");
        }

        return new ResponseModel<byte[]> { Data = Encoding.UTF8.GetBytes(csv.ToString()) };
    }

    public async Task WriteAuditLogAsync(WriteAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        await _platformRepository.WriteAuditLogAsync(request, cancellationToken);
    }

    // ── Settings ──

    public Task<ResponseModel<ClientSettingsDto>> GetSettingsAsync(int clientId, CancellationToken cancellationToken = default)
        => _platformRepository.GetClientSettingsAsync(clientId, cancellationToken);

    public async Task<ResponseModel<bool>> UpdateSettingsAsync(int clientId, UpdateClientSettingsRequest request, int userId, CancellationToken cancellationToken = default)
    {
        ResponseModel<bool> result = await _platformRepository.UpsertClientSettingsAsync(clientId, request, cancellationToken);
        if (result.Success)
        {
            await WriteAuditLogAsync(new WriteAuditLogRequest
            {
                UserId = userId,
                Action = "SETTINGS_UPDATED",
                EntityName = "CLIENT_SETTINGS",
                EntityId = clientId
            }, cancellationToken);
        }

        return result;
    }

    public async Task<ResponseModel<string>> UploadLogoAsync(int clientId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return new ResponseModel<string> { ErrorMessage = "Logo file is required.", ErrorId = -1 };
        }

        string logosDir = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "logos");
        Directory.CreateDirectory(logosDir);

        string fileName = $"{clientId}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        string fullPath = Path.Combine(logosDir, fileName);

        await using (FileStream stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        string logoUrl = $"/uploads/logos/{fileName}";
        ResponseModel<bool> updated = await _platformRepository.UpdateClientLogoAsync(clientId, logoUrl, cancellationToken);
        return updated.Success
            ? new ResponseModel<string> { Data = logoUrl }
            : new ResponseModel<string> { ErrorMessage = updated.ErrorMessage, ErrorId = updated.ErrorId };
    }

    // ── Engagement ──

    public Task<ResponseModel<PagedResponse<DirectoryMemberDto>>> GetDirectoryMembersAsync(int clientId, int page, int pageSize, string? search, bool includeContact, CancellationToken cancellationToken = default)
        => _platformRepository.GetDirectoryMembersAsync(clientId, NormalizePage(page), NormalizePageSize(pageSize), search, includeContact, cancellationToken);

    public Task<ResponseModel<DirectoryMemberDto>> GetDirectoryMemberAsync(int clientId, int memberId, bool includeContact, CancellationToken cancellationToken = default)
        => WrapNullable(_platformRepository.GetDirectoryMemberAsync(clientId, memberId, includeContact, cancellationToken));

    public Task<ResponseModel<IReadOnlyList<IndustryDto>>> GetIndustriesAsync(CancellationToken cancellationToken = default)
        => _platformRepository.GetIndustriesAsync(cancellationToken);

    public Task<ResponseModel<IReadOnlyList<EventListItemDto>>> GetEventsAsync(int clientId, CancellationToken cancellationToken = default)
        => _platformRepository.GetEventsAsync(clientId, cancellationToken);

    public Task<ResponseModel<EventDetailDto>> GetEventByIdAsync(int clientId, int eventId, int? memberId, CancellationToken cancellationToken = default)
        => WrapNullable(_platformRepository.GetEventByIdAsync(clientId, eventId, memberId, cancellationToken));

    public Task<ResponseModel<int>> CreateEventAsync(int clientId, CreateEventRequest request, int userId, CancellationToken cancellationToken = default)
        => _platformRepository.CreateEventAsync(clientId, request, userId, cancellationToken);

    public Task<ResponseModel<bool>> UpdateEventAsync(int clientId, int eventId, UpdateEventRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.UpdateEventAsync(clientId, eventId, request, cancellationToken);

    public Task<ResponseModel<bool>> DeleteEventAsync(int clientId, int eventId, CancellationToken cancellationToken = default)
        => _platformRepository.DeleteEventAsync(clientId, eventId, cancellationToken);

    public Task<ResponseModel<bool>> RsvpEventAsync(int eventId, int memberId, EventRsvpRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.UpsertEventRsvpAsync(eventId, memberId, request.Response, cancellationToken);

    public Task<ResponseModel<bool>> CancelEventRsvpAsync(int eventId, int memberId, CancellationToken cancellationToken = default)
        => _platformRepository.CancelEventRsvpAsync(eventId, memberId, cancellationToken);

    public async Task<ResponseModel<ReferralCodeResponse>> GetMyReferralCodeAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        ResponseModel<string> code = await _platformRepository.GetOrCreateReferralCodeAsync(clientId, memberId, cancellationToken);
        return code.Success
            ? new ResponseModel<ReferralCodeResponse> { Data = new ReferralCodeResponse { ReferralCode = code.Data } }
            : new ResponseModel<ReferralCodeResponse> { ErrorMessage = code.ErrorMessage, ErrorId = code.ErrorId };
    }

    public Task<ResponseModel<int>> TrackReferralAsync(int clientId, TrackReferralRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.TrackReferralAsync(clientId, request, cancellationToken);

    public Task<ResponseModel<ReferralStatsDto>> GetReferralStatsAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
        => _platformRepository.GetReferralStatsAsync(clientId, memberId, cancellationToken);

    public Task<ResponseModel<PagedResponse<ReferralListItemDto>>> GetReferralsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
        => _platformRepository.GetReferralsAsync(clientId, NormalizePage(page), NormalizePageSize(pageSize), cancellationToken);

    public Task<ResponseModel<int>> SubmitGrievanceAsync(int clientId, int memberId, SubmitGrievanceRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.SubmitGrievanceAsync(clientId, memberId, request, cancellationToken);

    public Task<ResponseModel<IReadOnlyList<GrievanceListItemDto>>> GetMyGrievancesAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
        => _platformRepository.GetMyGrievancesAsync(clientId, memberId, cancellationToken);

    public Task<ResponseModel<PagedResponse<GrievanceListItemDto>>> GetGrievancesAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
        => _platformRepository.GetGrievancesAsync(clientId, NormalizePage(page), NormalizePageSize(pageSize), cancellationToken);

    public Task<ResponseModel<bool>> UpdateGrievanceAsync(int clientId, int grievanceId, UpdateGrievanceRequest request, CancellationToken cancellationToken = default)
        => _platformRepository.UpdateGrievanceAsync(clientId, grievanceId, request, cancellationToken);

    // ── Helpers ──

    private static int NormalizePage(int page) => page < 1 ? 1 : page;
    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1) return 20;
        return pageSize > 100 ? 100 : pageSize;
    }

    private static async Task<ResponseModel<T>> WrapNullable<T>(Task<ResponseModel<T?>> task) where T : class
    {
        ResponseModel<T?> result = await task;
        if (!result.Success)
        {
            return new ResponseModel<T> { ErrorMessage = result.ErrorMessage, ErrorId = result.ErrorId };
        }

        if (result.Data == null)
        {
            return new ResponseModel<T> { ErrorMessage = "Not found.", ErrorId = -1 };
        }

        return new ResponseModel<T> { Data = result.Data };
    }
}
