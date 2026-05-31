using Dapper;
using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.IRepository;
using System.Data;

namespace MVEA.Repository.Repository;

public sealed class PlatformRepository : IPlatformRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<PlatformRepository> _logger;

    public PlatformRepository(ISqlConnectionFactory connectionFactory, ILogger<PlatformRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // ── Plans & Payments ──

    public async Task<ResponseModel<IReadOnlyList<MembershipPlanDto>>> GetPlansAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT p.PLAN_ID AS PlanId, p.PLAN_NAME AS Name, p.PRICE AS Price, p.VALIDITY_IN_MONTHS AS DurationMonths,
                       ISNULL(s.GST_PERCENT, 18) AS GstPercent, ISNULL(s.PLATFORM_FEE_FLAT, 50) AS PlatformFeeFlat
                FROM dbo.MEMBERSHIP_PLANS p
                LEFT JOIN dbo.CLIENT_SETTINGS s ON s.CLIENT_ID = p.CLIENT_ID
                WHERE p.CLIENT_ID = @ClientId AND ISNULL(p.IS_ACTIVE, 1) = 1
                ORDER BY p.PLAN_ID";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(sql, new { ClientId = clientId }, cancellationToken: cancellationToken));
            List<MembershipPlanDto> plans = rows.Select(r =>
            {
                decimal price = (decimal)r.Price;
                decimal gstPercent = (decimal)r.GstPercent;
                decimal platformFee = (decimal)r.PlatformFeeFlat;
                decimal gstAmount = Math.Round(price * gstPercent / 100m, 2);
                decimal total = price + gstAmount + platformFee;
                int? duration = r.DurationMonths == 0 ? null : (int?)r.DurationMonths;
                return new MembershipPlanDto
                {
                    PlanId = (int)r.PlanId,
                    Id = $"PLAN-{(int)r.PlanId:D3}",
                    Name = (string)r.Name,
                    Duration = duration,
                    Price = price,
                    GstPercent = gstPercent,
                    GstAmount = gstAmount,
                    PlatformFee = platformFee,
                    TotalAmount = total,
                    Features = new[] { "Digital ID Card", "Member Portal Access" }
                };
            }).ToList();

            return new ResponseModel<IReadOnlyList<MembershipPlanDto>> { Data = plans };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPlansAsync failed.");
            return new ResponseModel<IReadOnlyList<MembershipPlanDto>> { ErrorMessage = "Unable to load plans.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<MembershipPlanDto?>> GetPlanByIdAsync(int clientId, int planId, CancellationToken cancellationToken = default)
    {
        ResponseModel<IReadOnlyList<MembershipPlanDto>> plans = await GetPlansAsync(clientId, cancellationToken);
        if (!plans.Success)
        {
            return new ResponseModel<MembershipPlanDto?> { ErrorMessage = plans.ErrorMessage, ErrorId = plans.ErrorId };
        }

        MembershipPlanDto? plan = plans.Data?.FirstOrDefault(p => p.PlanId == planId);
        return plan == null
            ? new ResponseModel<MembershipPlanDto?> { ErrorMessage = "Plan not found.", ErrorId = -1 }
            : new ResponseModel<MembershipPlanDto?> { Data = plan };
    }

    public async Task<ResponseModel<int>> InsertPaymentAsync(int memberId, int planId, decimal amount, string orderId, string status, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                INSERT INTO dbo.PAYMENTS (MEMBER_ID, AMOUNT, PAYMENT_METHOD, PAYMENT_STATUS, RAZORPAY_ORDER_ID, CREATED_BY, CREATED_DATE)
                OUTPUT INSERTED.PAYMENT_ID
                VALUES (@MemberId, @Amount, N'Razorpay', @Status, @OrderId, @CreatedBy, SYSUTCDATETIME())";

            int paymentId = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new { MemberId = memberId, Amount = amount, Status = status, OrderId = orderId, CreatedBy = createdBy },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = paymentId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertPaymentAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to create payment record.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdatePaymentVerifiedAsync(string orderId, string paymentId, string receiptNo, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.PAYMENTS
                SET PAYMENT_STATUS = N'SUCCESS', RAZORPAY_PAYMENT_ID = @PaymentId, RECEIPT_NO = @ReceiptNo,
                    PAID_DATE = SYSUTCDATETIME(), MODIFIED_DATE = SYSUTCDATETIME()
                WHERE RAZORPAY_ORDER_ID = @OrderId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { OrderId = orderId, PaymentId = paymentId, ReceiptNo = receiptNo },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePaymentVerifiedAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to verify payment.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<PagedResponse<PaymentHistoryItemDto>>> GetPaymentHistoryAsync(int clientId, int memberId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                ;WITH q AS (
                    SELECT p.PAYMENT_ID AS PaymentId, ISNULL(p.RECEIPT_NO, N'') AS ReceiptNo, p.AMOUNT AS Amount,
                           ISNULL(p.PAYMENT_STATUS, N'') AS PaymentStatus, ISNULL(p.PAYMENT_METHOD, N'') AS PaymentMethod,
                           p.PAID_DATE AS PaidDate, N'' AS PlanName,
                           COUNT(*) OVER() AS Total
                    FROM dbo.PAYMENTS p
                    INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID
                    WHERE m.CLIENT_ID = @ClientId AND p.MEMBER_ID = @MemberId
                )
                SELECT * FROM q ORDER BY PaidDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(
                sql,
                new { ClientId = clientId, MemberId = memberId, Offset = (page - 1) * pageSize, PageSize = pageSize },
                cancellationToken: cancellationToken));

            List<dynamic> list = rows.ToList();
            int total = list.FirstOrDefault()?.Total ?? 0;
            List<PaymentHistoryItemDto> records = list.Select(r => new PaymentHistoryItemDto
            {
                PaymentId = (int)r.PaymentId,
                ReceiptNo = (string)r.ReceiptNo,
                Amount = (decimal)r.Amount,
                PaymentStatus = (string)r.PaymentStatus,
                PaymentMethod = (string)r.PaymentMethod,
                PaidDate = r.PaidDate as DateTime?,
                PlanName = (string)r.PlanName
            }).ToList();

            return new ResponseModel<PagedResponse<PaymentHistoryItemDto>>
            {
                Data = new PagedResponse<PaymentHistoryItemDto> { Total = total, Page = page, PageSize = pageSize, Records = records }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPaymentHistoryAsync failed.");
            return new ResponseModel<PagedResponse<PaymentHistoryItemDto>> { ErrorMessage = "Unable to load payment history.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<PaymentSummaryDto>> GetPaymentSummaryAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT ISNULL(SUM(CASE WHEN p.PAYMENT_STATUS = N'SUCCESS' THEN p.AMOUNT ELSE 0 END), 0) AS TotalPaid,
                       COUNT(CASE WHEN p.PAYMENT_STATUS = N'SUCCESS' THEN 1 END) AS PaymentCount,
                       MAX(CASE WHEN p.PAYMENT_STATUS = N'SUCCESS' THEN p.PAID_DATE END) AS LastPaymentDate
                FROM dbo.PAYMENTS p
                INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID
                WHERE m.CLIENT_ID = @ClientId AND p.MEMBER_ID = @MemberId";

            dynamic? row = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(
                sql, new { ClientId = clientId, MemberId = memberId }, cancellationToken: cancellationToken));

            return new ResponseModel<PaymentSummaryDto>
            {
                Data = new PaymentSummaryDto
                {
                    TotalPaid = row?.TotalPaid ?? 0m,
                    PaymentCount = row?.PaymentCount ?? 0,
                    LastPaymentDate = row?.LastPaymentDate as DateTime?
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPaymentSummaryAsync failed.");
            return new ResponseModel<PaymentSummaryDto> { ErrorMessage = "Unable to load payment summary.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdatePaymentRefundedAsync(int clientId, int paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE p SET p.PAYMENT_STATUS = N'REFUNDED', p.MODIFIED_DATE = SYSUTCDATETIME()
                FROM dbo.PAYMENTS p
                INNER JOIN dbo.MEMBERS m ON m.MEMBER_ID = p.MEMBER_ID
                WHERE p.PAYMENT_ID = @PaymentId AND m.CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { PaymentId = paymentId, ClientId = clientId }, cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePaymentRefundedAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to refund payment.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<(string Name, string Email, string Mobile)?>> GetMemberContactAsync(int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"SELECT OWNER_NAME AS Name, ISNULL(EMAIL, N'') AS Email, ISNULL(MOBILE_NUMBER, N'') AS Mobile
                                 FROM dbo.MEMBERS WHERE MEMBER_ID = @MemberId";
            dynamic? row = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(sql, new { MemberId = memberId }, cancellationToken: cancellationToken));
            if (row == null)
            {
                return new ResponseModel<(string Name, string Email, string Mobile)?> { ErrorMessage = "Member not found.", ErrorId = -1 };
            }

            return new ResponseModel<(string Name, string Email, string Mobile)?>
            {
                Data = ((string)row.Name, (string)row.Email, (string)row.Mobile)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMemberContactAsync failed.");
            return new ResponseModel<(string Name, string Email, string Mobile)?> { ErrorMessage = "Unable to load member.", ErrorId = -1 };
        }
    }

    // ── Registration ──

    public async Task<ResponseModel<int>> StartApplicationAsync(int clientId, StartRegistrationRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                INSERT INTO dbo.MEMBER_APPLICATIONS (CLIENT_ID, COMPANY_NAME, OWNER_NAME, EMAIL, MOBILE_NUMBER, STATUS, CURRENT_STEP, CREATED_BY, CREATED_DATE)
                OUTPUT INSERTED.APPLICATION_ID
                VALUES (@ClientId, @CompanyName, @OwnerName, @Email, @MobileNumber, N'DRAFT', 1, @CreatedBy, SYSUTCDATETIME())";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new { ClientId = clientId, request.CompanyName, request.OwnerName, request.Email, request.MobileNumber, CreatedBy = createdBy },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartApplicationAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to start registration.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateApplicationStepAsync(int clientId, int applicationId, RegistrationStepRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.MEMBER_APPLICATIONS
                SET CURRENT_STEP = @Step, DRAFT_DATA = @DraftData, MODIFIED_DATE = SYSUTCDATETIME()
                WHERE APPLICATION_ID = @ApplicationId AND CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { Step = request.Step, DraftData = request.DraftDataJson, ApplicationId = applicationId, ClientId = clientId },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateApplicationStepAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to save registration step.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> SubmitApplicationAsync(int clientId, int applicationId, SubmitRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.MEMBER_APPLICATIONS
                SET STATUS = N'PENDING', DRAFT_DATA = COALESCE(@FinalData, DRAFT_DATA), SUBMITTED_DATE = SYSUTCDATETIME(), MODIFIED_DATE = SYSUTCDATETIME()
                WHERE APPLICATION_ID = @ApplicationId AND CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new { FinalData = request.FinalDataJson, ApplicationId = applicationId, ClientId = clientId },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitApplicationAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to submit registration.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<RegistrationStatusResponse>> GetApplicationStatusAsync(int clientId, int applicationId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT APPLICATION_ID AS ApplicationId, ISNULL(CURRENT_STEP, 1) AS CurrentStep, ISNULL(STATUS, N'') AS Status,
                       DRAFT_DATA AS DraftDataJson, COMPANY_NAME AS CompanyName, ISNULL(OWNER_NAME, N'') AS OwnerName, SUBMITTED_DATE AS SubmittedDate
                FROM dbo.MEMBER_APPLICATIONS
                WHERE APPLICATION_ID = @ApplicationId AND CLIENT_ID = @ClientId";

            RegistrationStatusResponse? row = await connection.QueryFirstOrDefaultAsync<RegistrationStatusResponse>(
                new CommandDefinition(sql, new { ApplicationId = applicationId, ClientId = clientId }, cancellationToken: cancellationToken));

            return row == null
                ? new ResponseModel<RegistrationStatusResponse> { ErrorMessage = "Application not found.", ErrorId = -1 }
                : new ResponseModel<RegistrationStatusResponse> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetApplicationStatusAsync failed.");
            return new ResponseModel<RegistrationStatusResponse> { ErrorMessage = "Unable to load registration status.", ErrorId = -1 };
        }
    }

    // ── Documents ──

    public async Task<ResponseModel<int>> InsertDocumentAsync(int clientId, int applicationId, string documentType, string blobUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                INSERT INTO dbo.APPLICATION_DOCUMENTS (CLIENT_ID, APPLICATION_ID, DOCUMENT_TYPE, BLOB_URL, STATUS)
                OUTPUT INSERTED.DOCUMENT_ID
                VALUES (@ClientId, @ApplicationId, @DocumentType, @BlobUrl, N'PROCESSING')";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new { ClientId = clientId, ApplicationId = applicationId, DocumentType = documentType, BlobUrl = blobUrl },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertDocumentAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to upload document.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<IReadOnlyList<DocumentListItemDto>>> GetDocumentsAsync(int clientId, int applicationId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT DOCUMENT_ID AS DocumentId, APPLICATION_ID AS ApplicationId, DOCUMENT_TYPE AS DocumentType,
                       BLOB_URL AS BlobUrl, STATUS AS Status, AI_VALID AS AiValid, AI_CONFIDENCE AS AiConfidence,
                       AI_REASON AS AiReason, UPLOADED_AT AS UploadedAt
                FROM dbo.APPLICATION_DOCUMENTS
                WHERE CLIENT_ID = @ClientId AND APPLICATION_ID = @ApplicationId
                ORDER BY UPLOADED_AT DESC";

            IEnumerable<DocumentListItemDto> rows = await connection.QueryAsync<DocumentListItemDto>(
                new CommandDefinition(sql, new { ClientId = clientId, ApplicationId = applicationId }, cancellationToken: cancellationToken));

            return new ResponseModel<IReadOnlyList<DocumentListItemDto>> { Data = rows.ToList() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDocumentsAsync failed.");
            return new ResponseModel<IReadOnlyList<DocumentListItemDto>> { ErrorMessage = "Unable to load documents.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<DocumentListItemDto?>> GetDocumentByIdAsync(int clientId, int documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT DOCUMENT_ID AS DocumentId, APPLICATION_ID AS ApplicationId, DOCUMENT_TYPE AS DocumentType,
                       BLOB_URL AS BlobUrl, STATUS AS Status, AI_VALID AS AiValid, AI_CONFIDENCE AS AiConfidence,
                       AI_REASON AS AiReason, UPLOADED_AT AS UploadedAt
                FROM dbo.APPLICATION_DOCUMENTS
                WHERE CLIENT_ID = @ClientId AND DOCUMENT_ID = @DocumentId";

            DocumentListItemDto? row = await connection.QueryFirstOrDefaultAsync<DocumentListItemDto>(
                new CommandDefinition(sql, new { ClientId = clientId, DocumentId = documentId }, cancellationToken: cancellationToken));

            return row == null
                ? new ResponseModel<DocumentListItemDto?> { ErrorMessage = "Document not found.", ErrorId = -1 }
                : new ResponseModel<DocumentListItemDto?> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDocumentByIdAsync failed.");
            return new ResponseModel<DocumentListItemDto?> { ErrorMessage = "Unable to load document.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateDocumentAiResultAsync(int documentId, bool aiValid, decimal confidence, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.APPLICATION_DOCUMENTS
                SET AI_VALID = @AiValid, AI_CONFIDENCE = @Confidence, AI_REASON = @Reason, STATUS = N'AI_REVIEWED'
                WHERE DOCUMENT_ID = @DocumentId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { DocumentId = documentId, AiValid = aiValid, Confidence = confidence, Reason = reason },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateDocumentAiResultAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update AI verification.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> VerifyDocumentAsync(int clientId, int documentId, VerifyDocumentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.APPLICATION_DOCUMENTS
                SET STATUS = @Status, REMARKS = @Remarks
                WHERE DOCUMENT_ID = @DocumentId AND CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { request.Status, request.Remarks, DocumentId = documentId, ClientId = clientId },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyDocumentAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to verify document.", ErrorId = -1 };
        }
    }

    // ── Digital ID ──

    public async Task<ResponseModel<DigitalIdResponse?>> GetDigitalIdAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT d.MEMBER_ID AS MemberId, ISNULL(d.MEMBERSHIP_ID, m.MEMBERSHIP_ID) AS MembershipId,
                       m.OWNER_NAME AS OwnerName, d.PHOTO_URL AS PhotoUrl, d.QR_CODE_URL AS QrCodeUrl, d.QR_VALUE AS QrValue,
                       d.DESIGNATION AS Designation, d.VALID_FROM AS ValidFrom, d.VALID_UNTIL AS ValidUntil,
                       ISNULL(d.STATUS, N'ACTIVE') AS Status, ISNULL(d.IS_GENERATED, 0) AS IsGenerated
                FROM dbo.MEMBERS m
                LEFT JOIN dbo.DIGITAL_MEMBER_IDS d ON d.MEMBER_ID = m.MEMBER_ID
                WHERE m.MEMBER_ID = @MemberId AND m.CLIENT_ID = @ClientId";

            DigitalIdResponse? row = await connection.QueryFirstOrDefaultAsync<DigitalIdResponse>(
                new CommandDefinition(sql, new { MemberId = memberId, ClientId = clientId }, cancellationToken: cancellationToken));

            return row == null
                ? new ResponseModel<DigitalIdResponse?> { ErrorMessage = "Member not found.", ErrorId = -1 }
                : new ResponseModel<DigitalIdResponse?> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDigitalIdAsync failed.");
            return new ResponseModel<DigitalIdResponse?> { ErrorMessage = "Unable to load digital ID.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpsertDigitalIdAsync(int clientId, int memberId, DigitalIdResponse data, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                MERGE dbo.DIGITAL_MEMBER_IDS AS t
                USING (SELECT @MemberId AS MEMBER_ID) AS s ON t.MEMBER_ID = s.MEMBER_ID
                WHEN MATCHED THEN UPDATE SET
                    MEMBERSHIP_ID = @MembershipId, QR_VALUE = @QrValue, QR_CODE_URL = @QrCodeUrl, PHOTO_URL = @PhotoUrl,
                    DESIGNATION = @Designation, VALID_FROM = @ValidFrom, VALID_UNTIL = @ValidUntil,
                    STATUS = @Status, IS_GENERATED = 1, GENERATED_AT = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT
                    (MEMBER_ID, CLIENT_ID, MEMBERSHIP_ID, QR_VALUE, QR_CODE_URL, PHOTO_URL, DESIGNATION, VALID_FROM, VALID_UNTIL, STATUS, IS_GENERATED, GENERATED_AT)
                VALUES (@MemberId, @ClientId, @MembershipId, @QrValue, @QrCodeUrl, @PhotoUrl, @Designation, @ValidFrom, @ValidUntil, @Status, 1, SYSUTCDATETIME());";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    MemberId = memberId,
                    ClientId = clientId,
                    data.MembershipId,
                    data.QrValue,
                    data.QrCodeUrl,
                    data.PhotoUrl,
                    data.Designation,
                    data.ValidFrom,
                    data.ValidUntil,
                    data.Status
                },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpsertDigitalIdAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to generate digital ID.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<DigitalIdVerifyResponse?>> VerifyDigitalIdAsync(string membershipId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT ISNULL(d.MEMBERSHIP_ID, m.MEMBERSHIP_ID) AS MembershipId, m.OWNER_NAME AS OwnerName,
                       ISNULL(d.STATUS, N'ACTIVE') AS Status, d.VALID_UNTIL AS ValidUntil
                FROM dbo.MEMBERS m
                LEFT JOIN dbo.DIGITAL_MEMBER_IDS d ON d.MEMBER_ID = m.MEMBER_ID
                WHERE m.MEMBERSHIP_ID = @MembershipId OR d.MEMBERSHIP_ID = @MembershipId";

            dynamic? row = await connection.QueryFirstOrDefaultAsync(new CommandDefinition(
                sql, new { MembershipId = membershipId }, cancellationToken: cancellationToken));

            if (row == null)
            {
                return new ResponseModel<DigitalIdVerifyResponse?> { Data = new DigitalIdVerifyResponse { Valid = false } };
            }

            DateTime? validUntil = row.ValidUntil as DateTime?;
            bool valid = (string)row.Status == "ACTIVE" && (validUntil == null || validUntil >= DateTime.UtcNow.Date);

            return new ResponseModel<DigitalIdVerifyResponse?>
            {
                Data = new DigitalIdVerifyResponse
                {
                    Valid = valid,
                    MembershipId = (string)row.MembershipId,
                    OwnerName = (string)row.OwnerName,
                    Status = (string)row.Status,
                    ValidUntil = validUntil
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyDigitalIdAsync failed.");
            return new ResponseModel<DigitalIdVerifyResponse?> { ErrorMessage = "Unable to verify digital ID.", ErrorId = -1 };
        }
    }

    // ── Audit ──

    public async Task<ResponseModel<PagedResponse<AuditLogListItemDto>>> GetAuditLogsAsync(int clientId, AuditLogFilterRequest filter, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                ;WITH q AS (
                    SELECT a.AUDIT_ID AS AuditId, a.USER_ID AS UserId, ISNULL(a.ACTION, N'') AS Action,
                           ISNULL(a.ENTITY_NAME, N'') AS EntityName, a.ENTITY_ID AS EntityId,
                           a.TARGET_LABEL AS TargetLabel, a.STAFF_ROLE AS StaffRole, a.IP_ADDRESS AS IpAddress,
                           ISNULL(a.ACTION_DATE, a.CREATED_DATE) AS ActionDate,
                           COUNT(*) OVER() AS Total
                    FROM dbo.AUDIT_LOGS a
                    INNER JOIN dbo.USERS u ON u.USER_ID = a.USER_ID
                    WHERE u.CLIENT_ID = @ClientId
                      AND (@StaffId IS NULL OR a.USER_ID = @StaffId)
                      AND (@ActionType IS NULL OR a.ACTION LIKE '%' + @ActionType + '%')
                      AND (@EntityType IS NULL OR a.ENTITY_NAME = @EntityType)
                      AND (@DateFrom IS NULL OR a.ACTION_DATE >= @DateFrom)
                      AND (@DateTo IS NULL OR a.ACTION_DATE <= @DateTo)
                )
                SELECT AuditId, UserId, Action, EntityName, EntityId, TargetLabel, StaffRole, IpAddress, ActionDate, Total
                FROM q ORDER BY ActionDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(
                sql,
                new
                {
                    ClientId = clientId,
                    filter.StaffId,
                    filter.ActionType,
                    filter.EntityType,
                    filter.DateFrom,
                    filter.DateTo,
                    Offset = (filter.Page - 1) * filter.PageSize,
                    PageSize = filter.PageSize
                },
                cancellationToken: cancellationToken));

            List<dynamic> list = rows.ToList();
            int total = list.FirstOrDefault()?.Total ?? 0;
            List<AuditLogListItemDto> records = list.Select(r => new AuditLogListItemDto
            {
                AuditId = (int)r.AuditId,
                UserId = r.UserId as int?,
                Action = (string)r.Action,
                EntityName = (string)r.EntityName,
                EntityId = r.EntityId as int?,
                TargetLabel = r.TargetLabel as string,
                StaffRole = r.StaffRole as string,
                IpAddress = r.IpAddress as string,
                ActionDate = (DateTime)r.ActionDate
            }).ToList();

            return new ResponseModel<PagedResponse<AuditLogListItemDto>>
            {
                Data = new PagedResponse<AuditLogListItemDto>
                {
                    Total = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    Records = records
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAuditLogsAsync failed.");
            return new ResponseModel<PagedResponse<AuditLogListItemDto>> { ErrorMessage = "Unable to load audit logs.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> WriteAuditLogAsync(WriteAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                INSERT INTO dbo.AUDIT_LOGS (USER_ID, ACTION, ENTITY_NAME, ENTITY_ID, TARGET_LABEL, STAFF_ROLE, IP_ADDRESS, CHANGES_JSON, CREATED_BY, CREATED_DATE, ACTION_DATE)
                OUTPUT INSERTED.AUDIT_ID
                VALUES (@UserId, @Action, @EntityName, @EntityId, @TargetLabel, @StaffRole, @IpAddress, @ChangesJson, @UserId, SYSUTCDATETIME(), SYSUTCDATETIME())";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new
                {
                    request.UserId,
                    request.Action,
                    request.EntityName,
                    request.EntityId,
                    request.TargetLabel,
                    request.StaffRole,
                    request.IpAddress,
                    request.ChangesJson
                },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WriteAuditLogAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to write audit log.", ErrorId = -1 };
        }
    }

    // ── Settings ──

    public async Task<ResponseModel<ClientSettingsDto>> GetClientSettingsAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT CLIENT_ID AS ClientId, ISNULL(ASSOCIATION_NAME, N'') AS AssociationName, LOGO_URL AS LogoUrl,
                       PRIMARY_COLOR AS PrimaryColor, YEARLY_FEE AS YearlyFee, LIFETIME_FEE AS LifetimeFee,
                       GST_PERCENT AS GstPercent, PLATFORM_FEE_FLAT AS PlatformFeeFlat, GST_NUMBER AS GstNumber,
                       ADDRESS AS Address, SUPPORT_PHONE AS SupportPhone, SUPPORT_EMAIL AS SupportEmail,
                       ISNULL(WHATSAPP_ENABLED, 1) AS WhatsappEnabled, ISNULL(SMS_ENABLED, 1) AS SmsEnabled,
                       ISNULL(AUTO_APPROVAL, 0) AS AutoApproval, ISNULL(RENEWAL_REMINDER_DAYS, 30) AS RenewalReminderDays
                FROM dbo.CLIENT_SETTINGS WHERE CLIENT_ID = @ClientId";

            ClientSettingsDto? row = await connection.QueryFirstOrDefaultAsync<ClientSettingsDto>(
                new CommandDefinition(sql, new { ClientId = clientId }, cancellationToken: cancellationToken));

            if (row == null)
            {
                row = new ClientSettingsDto { ClientId = clientId };
            }

            return new ResponseModel<ClientSettingsDto> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetClientSettingsAsync failed.");
            return new ResponseModel<ClientSettingsDto> { ErrorMessage = "Unable to load settings.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpsertClientSettingsAsync(int clientId, UpdateClientSettingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                MERGE dbo.CLIENT_SETTINGS AS t
                USING (SELECT @ClientId AS CLIENT_ID) AS s ON t.CLIENT_ID = s.CLIENT_ID
                WHEN MATCHED THEN UPDATE SET
                    ASSOCIATION_NAME = COALESCE(@AssociationName, ASSOCIATION_NAME),
                    PRIMARY_COLOR = COALESCE(@PrimaryColor, PRIMARY_COLOR),
                    YEARLY_FEE = COALESCE(@YearlyFee, YEARLY_FEE),
                    LIFETIME_FEE = COALESCE(@LifetimeFee, LIFETIME_FEE),
                    GST_PERCENT = COALESCE(@GstPercent, GST_PERCENT),
                    PLATFORM_FEE_FLAT = COALESCE(@PlatformFeeFlat, PLATFORM_FEE_FLAT),
                    GST_NUMBER = COALESCE(@GstNumber, GST_NUMBER),
                    ADDRESS = COALESCE(@Address, ADDRESS),
                    SUPPORT_PHONE = COALESCE(@SupportPhone, SUPPORT_PHONE),
                    SUPPORT_EMAIL = COALESCE(@SupportEmail, SUPPORT_EMAIL),
                    WHATSAPP_ENABLED = COALESCE(@WhatsappEnabled, WHATSAPP_ENABLED),
                    SMS_ENABLED = COALESCE(@SmsEnabled, SMS_ENABLED),
                    AUTO_APPROVAL = COALESCE(@AutoApproval, AUTO_APPROVAL),
                    RENEWAL_REMINDER_DAYS = COALESCE(@RenewalReminderDays, RENEWAL_REMINDER_DAYS),
                    MODIFIED_DATE = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT
                    (CLIENT_ID, ASSOCIATION_NAME, PRIMARY_COLOR, YEARLY_FEE, LIFETIME_FEE, GST_PERCENT, PLATFORM_FEE_FLAT,
                     GST_NUMBER, ADDRESS, SUPPORT_PHONE, SUPPORT_EMAIL, WHATSAPP_ENABLED, SMS_ENABLED, AUTO_APPROVAL, RENEWAL_REMINDER_DAYS)
                VALUES (@ClientId, @AssociationName, @PrimaryColor, @YearlyFee, @LifetimeFee, @GstPercent, @PlatformFeeFlat,
                        @GstNumber, @Address, @SupportPhone, @SupportEmail, @WhatsappEnabled, @SmsEnabled, @AutoApproval, @RenewalReminderDays);";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    ClientId = clientId,
                    request.AssociationName,
                    request.PrimaryColor,
                    request.YearlyFee,
                    request.LifetimeFee,
                    request.GstPercent,
                    request.PlatformFeeFlat,
                    request.GstNumber,
                    request.Address,
                    request.SupportPhone,
                    request.SupportEmail,
                    WhatsappEnabled = request.WhatsappEnabled ?? true,
                    SmsEnabled = request.SmsEnabled ?? true,
                    AutoApproval = request.AutoApproval ?? false,
                    RenewalReminderDays = request.RenewalReminderDays ?? 30
                },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpsertClientSettingsAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update settings.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateClientLogoAsync(int clientId, string logoUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                MERGE dbo.CLIENT_SETTINGS AS t
                USING (SELECT @ClientId AS CLIENT_ID) AS s ON t.CLIENT_ID = s.CLIENT_ID
                WHEN MATCHED THEN UPDATE SET LOGO_URL = @LogoUrl, MODIFIED_DATE = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT (CLIENT_ID, LOGO_URL) VALUES (@ClientId, @LogoUrl);";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { ClientId = clientId, LogoUrl = logoUrl }, cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateClientLogoAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update logo.", ErrorId = -1 };
        }
    }

    // ── Directory ──

    public async Task<ResponseModel<PagedResponse<DirectoryMemberDto>>> GetDirectoryMembersAsync(int clientId, int page, int pageSize, string? search, bool includeContact, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                ;WITH q AS (
                    SELECT m.MEMBER_ID AS MemberId, ISNULL(m.MEMBERSHIP_ID, N'') AS MembershipId,
                           ISNULL(m.OWNER_NAME, N'') AS OwnerName, m.CITY AS City,
                           c.COMPANY_NAME AS CompanyName,
                           CASE WHEN @IncludeContact = 1 THEN m.MOBILE_NUMBER ELSE NULL END AS Phone,
                           CASE WHEN @IncludeContact = 1 THEN m.EMAIL ELSE NULL END AS Email,
                           COUNT(*) OVER() AS Total
                    FROM dbo.MEMBERS m
                    INNER JOIN dbo.COMPANY_MASTER c ON c.COMPANY_ID = m.COMPANY_ID
                    WHERE m.CLIENT_ID = @ClientId AND ISNULL(m.IS_ACTIVE, 1) = 1
                      AND (@Search IS NULL OR m.OWNER_NAME LIKE '%' + @Search + '%' OR m.MEMBERSHIP_ID LIKE '%' + @Search + '%')
                )
                SELECT MemberId, MembershipId, OwnerName, City, CompanyName, Phone, Email, Total
                FROM q ORDER BY OwnerName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(
                sql,
                new { ClientId = clientId, IncludeContact = includeContact ? 1 : 0, Search = search, Offset = (page - 1) * pageSize, PageSize = pageSize },
                cancellationToken: cancellationToken));

            List<dynamic> list = rows.ToList();
            int total = list.FirstOrDefault()?.Total ?? 0;
            List<DirectoryMemberDto> records = list.Select(r => new DirectoryMemberDto
            {
                MemberId = (int)r.MemberId,
                MembershipId = (string)r.MembershipId,
                OwnerName = (string)r.OwnerName,
                City = r.City as string,
                CompanyName = r.CompanyName as string,
                Phone = r.Phone as string,
                Email = r.Email as string
            }).ToList();

            return new ResponseModel<PagedResponse<DirectoryMemberDto>>
            {
                Data = new PagedResponse<DirectoryMemberDto> { Total = total, Page = page, PageSize = pageSize, Records = records }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDirectoryMembersAsync failed.");
            return new ResponseModel<PagedResponse<DirectoryMemberDto>> { ErrorMessage = "Unable to load directory.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<DirectoryMemberDto?>> GetDirectoryMemberAsync(int clientId, int memberId, bool includeContact, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT m.MEMBER_ID AS MemberId, ISNULL(m.MEMBERSHIP_ID, N'') AS MembershipId,
                       ISNULL(m.OWNER_NAME, N'') AS OwnerName, m.CITY AS City,
                       c.COMPANY_NAME AS CompanyName,
                       CASE WHEN @IncludeContact = 1 THEN m.MOBILE_NUMBER ELSE NULL END AS Phone,
                       CASE WHEN @IncludeContact = 1 THEN m.EMAIL ELSE NULL END AS Email
                FROM dbo.MEMBERS m
                INNER JOIN dbo.COMPANY_MASTER c ON c.COMPANY_ID = m.COMPANY_ID
                WHERE m.CLIENT_ID = @ClientId AND m.MEMBER_ID = @MemberId AND ISNULL(m.IS_ACTIVE, 1) = 1";

            DirectoryMemberDto? member = await connection.QueryFirstOrDefaultAsync<DirectoryMemberDto>(
                new CommandDefinition(sql, new { ClientId = clientId, MemberId = memberId, IncludeContact = includeContact ? 1 : 0 }, cancellationToken: cancellationToken));

            return member == null
                ? new ResponseModel<DirectoryMemberDto?> { ErrorMessage = "Member not found.", ErrorId = -1 }
                : new ResponseModel<DirectoryMemberDto?> { Data = member };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDirectoryMemberAsync failed.");
            return new ResponseModel<DirectoryMemberDto?> { ErrorMessage = "Unable to load member.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<IReadOnlyList<IndustryDto>>> GetIndustriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"SELECT COMPANY_TYPE_ID AS CompanyTypeId, ISNULL(NAME, N'') AS Name
                                 FROM dbo.COMPANY_TYPE ORDER BY NAME";

            IEnumerable<IndustryDto> rows = await connection.QueryAsync<IndustryDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return new ResponseModel<IReadOnlyList<IndustryDto>> { Data = rows.ToList() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetIndustriesAsync failed.");
            return new ResponseModel<IReadOnlyList<IndustryDto>> { ErrorMessage = "Unable to load industries.", ErrorId = -1 };
        }
    }

    // ── Events ──

    public async Task<ResponseModel<IReadOnlyList<EventListItemDto>>> GetEventsAsync(int clientId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT EVENT_ID AS EventId, TITLE AS Title, EVENT_TYPE AS EventType, ISNULL(STATUS, N'') AS Status,
                       EVENT_DATE AS EventDate, EVENT_TIME AS EventTime, VENUE AS Venue,
                       ISNULL(TOTAL_SEATS, 0) AS TotalSeats, ISNULL(BOOKED_SEATS, 0) AS BookedSeats,
                       ISNULL(TICKET_PRICE, 0) AS TicketPrice, ISNULL(IS_FREE, 1) AS IsFree, ISNULL(IS_ONLINE, 0) AS IsOnline
                FROM dbo.EVENTS WHERE CLIENT_ID = @ClientId ORDER BY EVENT_DATE";

            IEnumerable<EventListItemDto> rows = await connection.QueryAsync<EventListItemDto>(
                new CommandDefinition(sql, new { ClientId = clientId }, cancellationToken: cancellationToken));

            return new ResponseModel<IReadOnlyList<EventListItemDto>> { Data = rows.ToList() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventsAsync failed.");
            return new ResponseModel<IReadOnlyList<EventListItemDto>> { ErrorMessage = "Unable to load events.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<EventDetailDto?>> GetEventByIdAsync(int clientId, int eventId, int? memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT e.EVENT_ID AS EventId, e.TITLE AS Title, e.EVENT_TYPE AS EventType, ISNULL(e.STATUS, N'') AS Status,
                       e.EVENT_DATE AS EventDate, e.EVENT_TIME AS EventTime, e.VENUE AS Venue,
                       ISNULL(e.TOTAL_SEATS, 0) AS TotalSeats, ISNULL(e.BOOKED_SEATS, 0) AS BookedSeats,
                       ISNULL(e.TICKET_PRICE, 0) AS TicketPrice, ISNULL(e.IS_FREE, 1) AS IsFree, ISNULL(e.IS_ONLINE, 0) AS IsOnline,
                       e.DESCRIPTION AS Description, e.MEET_LINK AS MeetLink, e.RSVP_DEADLINE AS RsvpDeadline,
                       r.RESPONSE AS MyRsvp
                FROM dbo.EVENTS e
                LEFT JOIN dbo.EVENT_RSVPS r ON r.EVENT_ID = e.EVENT_ID AND r.MEMBER_ID = @MemberId
                WHERE e.CLIENT_ID = @ClientId AND e.EVENT_ID = @EventId";

            EventDetailDto? row = await connection.QueryFirstOrDefaultAsync<EventDetailDto>(
                new CommandDefinition(sql, new { ClientId = clientId, EventId = eventId, MemberId = memberId ?? 0 }, cancellationToken: cancellationToken));

            return row == null
                ? new ResponseModel<EventDetailDto?> { ErrorMessage = "Event not found.", ErrorId = -1 }
                : new ResponseModel<EventDetailDto?> { Data = row };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventByIdAsync failed.");
            return new ResponseModel<EventDetailDto?> { ErrorMessage = "Unable to load event.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> CreateEventAsync(int clientId, CreateEventRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                INSERT INTO dbo.EVENTS (CLIENT_ID, TITLE, DESCRIPTION, EVENT_TYPE, EVENT_DATE, EVENT_TIME, VENUE,
                    TOTAL_SEATS, TICKET_PRICE, IS_FREE, IS_ONLINE, MEET_LINK, RSVP_DEADLINE, CREATED_BY)
                OUTPUT INSERTED.EVENT_ID
                VALUES (@ClientId, @Title, @Description, @EventType, @EventDate, @EventTime, @Venue,
                    @TotalSeats, @TicketPrice, @IsFree, @IsOnline, @MeetLink, @RsvpDeadline, @CreatedBy)";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new
                {
                    ClientId = clientId,
                    request.Title,
                    request.Description,
                    request.EventType,
                    request.EventDate,
                    request.EventTime,
                    request.Venue,
                    request.TotalSeats,
                    request.TicketPrice,
                    request.IsFree,
                    request.IsOnline,
                    request.MeetLink,
                    request.RsvpDeadline,
                    CreatedBy = createdBy
                },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateEventAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to create event.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateEventAsync(int clientId, int eventId, UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.EVENTS SET
                    TITLE = @Title, DESCRIPTION = @Description, EVENT_TYPE = @EventType, STATUS = COALESCE(@Status, STATUS),
                    EVENT_DATE = @EventDate, EVENT_TIME = @EventTime, VENUE = @Venue, TOTAL_SEATS = @TotalSeats,
                    TICKET_PRICE = @TicketPrice, IS_FREE = @IsFree, IS_ONLINE = @IsOnline, MEET_LINK = @MeetLink, RSVP_DEADLINE = @RsvpDeadline
                WHERE EVENT_ID = @EventId AND CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    EventId = eventId,
                    ClientId = clientId,
                    request.Title,
                    request.Description,
                    request.EventType,
                    request.Status,
                    request.EventDate,
                    request.EventTime,
                    request.Venue,
                    request.TotalSeats,
                    request.TicketPrice,
                    request.IsFree,
                    request.IsOnline,
                    request.MeetLink,
                    request.RsvpDeadline
                },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEventAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update event.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> DeleteEventAsync(int clientId, int eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"UPDATE dbo.EVENTS SET STATUS = N'CANCELLED' WHERE EVENT_ID = @EventId AND CLIENT_ID = @ClientId";
            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { EventId = eventId, ClientId = clientId }, cancellationToken: cancellationToken));
            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEventAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to delete event.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpsertEventRsvpAsync(int eventId, int memberId, string response, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                MERGE dbo.EVENT_RSVPS AS t
                USING (SELECT @EventId AS EVENT_ID, @MemberId AS MEMBER_ID) AS s
                ON t.EVENT_ID = s.EVENT_ID AND t.MEMBER_ID = s.MEMBER_ID
                WHEN MATCHED THEN UPDATE SET RESPONSE = @Response, RSVP_AT = SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT (EVENT_ID, MEMBER_ID, RESPONSE) VALUES (@EventId, @MemberId, @Response);

                UPDATE dbo.EVENTS SET BOOKED_SEATS = (
                    SELECT COUNT(*) FROM dbo.EVENT_RSVPS WHERE EVENT_ID = @EventId AND RESPONSE = N'YES')
                WHERE EVENT_ID = @EventId;";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { EventId = eventId, MemberId = memberId, Response = response }, cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpsertEventRsvpAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to RSVP.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> CancelEventRsvpAsync(int eventId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                DELETE FROM dbo.EVENT_RSVPS WHERE EVENT_ID = @EventId AND MEMBER_ID = @MemberId;
                UPDATE dbo.EVENTS SET BOOKED_SEATS = (
                    SELECT COUNT(*) FROM dbo.EVENT_RSVPS WHERE EVENT_ID = @EventId AND RESPONSE = N'YES')
                WHERE EVENT_ID = @EventId;";

            await connection.ExecuteAsync(new CommandDefinition(
                sql, new { EventId = eventId, MemberId = memberId }, cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelEventRsvpAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to cancel RSVP.", ErrorId = -1 };
        }
    }

    // ── Referrals ──

    public async Task<ResponseModel<string>> GetOrCreateReferralCodeAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string selectSql = @"SELECT TOP 1 REFERRAL_CODE FROM dbo.MEMBER_REFERRALS
                                       WHERE CLIENT_ID = @ClientId AND REFERRER_MEMBER_ID = @MemberId
                                       ORDER BY REFERRAL_ID";
            string? existing = await connection.QueryFirstOrDefaultAsync<string>(
                new CommandDefinition(selectSql, new { ClientId = clientId, MemberId = memberId }, cancellationToken: cancellationToken));

            if (!string.IsNullOrEmpty(existing))
            {
                return new ResponseModel<string> { Data = existing };
            }

            string code = $"REF-{memberId:D5}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            const string insertSql = @"
                INSERT INTO dbo.MEMBER_REFERRALS (CLIENT_ID, REFERRER_MEMBER_ID, REFERRAL_CODE, REFEREE_NAME, REFEREE_PHONE, STATUS)
                VALUES (@ClientId, @MemberId, @Code, N'SELF', N'0000000000', N'ACTIVE')";

            await connection.ExecuteAsync(new CommandDefinition(
                insertSql, new { ClientId = clientId, MemberId = memberId, Code = code }, cancellationToken: cancellationToken));

            return new ResponseModel<string> { Data = code };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrCreateReferralCodeAsync failed.");
            return new ResponseModel<string> { ErrorMessage = "Unable to get referral code.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<int>> TrackReferralAsync(int clientId, TrackReferralRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string findSql = @"SELECT TOP 1 REFERRER_MEMBER_ID FROM dbo.MEMBER_REFERRALS
                                     WHERE CLIENT_ID = @ClientId AND REFERRAL_CODE = @ReferralCode";
            int? referrerId = await connection.QueryFirstOrDefaultAsync<int?>(
                new CommandDefinition(findSql, new { ClientId = clientId, request.ReferralCode }, cancellationToken: cancellationToken));

            if (referrerId == null)
            {
                return new ResponseModel<int> { ErrorMessage = "Invalid referral code.", ErrorId = -1 };
            }

            const string insertSql = @"
                INSERT INTO dbo.MEMBER_REFERRALS (CLIENT_ID, REFERRER_MEMBER_ID, REFERRAL_CODE, REFEREE_NAME, REFEREE_PHONE, REFEREE_FIRM, STATUS)
                OUTPUT INSERTED.REFERRAL_ID
                VALUES (@ClientId, @ReferrerId, @ReferralCode, @RefereeName, @RefereePhone, @RefereeFirm, N'PENDING')";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                insertSql,
                new { ClientId = clientId, ReferrerId = referrerId, request.ReferralCode, request.RefereeName, request.RefereePhone, request.RefereeFirm },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TrackReferralAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to track referral.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<ReferralStatsDto>> GetReferralStatsAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT COUNT(*) AS TotalReferrals,
                       SUM(CASE WHEN STATUS = N'PENDING' THEN 1 ELSE 0 END) AS PendingCount,
                       SUM(CASE WHEN STATUS IN (N'APPROVED', N'CONVERTED') THEN 1 ELSE 0 END) AS ApprovedCount
                FROM dbo.MEMBER_REFERRALS
                WHERE CLIENT_ID = @ClientId AND REFERRER_MEMBER_ID = @MemberId AND REFEREE_NAME <> N'SELF'";

            ReferralStatsDto? row = await connection.QueryFirstOrDefaultAsync<ReferralStatsDto>(
                new CommandDefinition(sql, new { ClientId = clientId, MemberId = memberId }, cancellationToken: cancellationToken));

            return new ResponseModel<ReferralStatsDto>
            {
                Data = row ?? new ReferralStatsDto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReferralStatsAsync failed.");
            return new ResponseModel<ReferralStatsDto> { ErrorMessage = "Unable to load referral stats.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<PagedResponse<ReferralListItemDto>>> GetReferralsAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                ;WITH q AS (
                    SELECT REFERRAL_ID AS ReferralId, REFERRAL_CODE AS ReferralCode, REFEREE_NAME AS RefereeName,
                           REFEREE_PHONE AS RefereePhone, REFEREE_FIRM AS RefereeFirm, ISNULL(STATUS, N'') AS Status,
                           APPLIED_AT AS AppliedAt, COUNT(*) OVER() AS Total
                    FROM dbo.MEMBER_REFERRALS
                    WHERE CLIENT_ID = @ClientId AND REFEREE_NAME <> N'SELF'
                )
                SELECT * FROM q ORDER BY AppliedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(
                sql, new { ClientId = clientId, Offset = (page - 1) * pageSize, PageSize = pageSize }, cancellationToken: cancellationToken));

            List<dynamic> list = rows.ToList();
            int total = list.FirstOrDefault()?.Total ?? 0;
            List<ReferralListItemDto> records = list.Select(r => new ReferralListItemDto
            {
                ReferralId = (int)r.ReferralId,
                ReferralCode = (string)r.ReferralCode,
                RefereeName = (string)r.RefereeName,
                RefereePhone = (string)r.RefereePhone,
                RefereeFirm = r.RefereeFirm as string,
                Status = (string)r.Status,
                AppliedAt = (DateTime)r.AppliedAt
            }).ToList();

            return new ResponseModel<PagedResponse<ReferralListItemDto>>
            {
                Data = new PagedResponse<ReferralListItemDto> { Total = total, Page = page, PageSize = pageSize, Records = records }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReferralsAsync failed.");
            return new ResponseModel<PagedResponse<ReferralListItemDto>> { ErrorMessage = "Unable to load referrals.", ErrorId = -1 };
        }
    }

    // ── Grievances ──

    public async Task<ResponseModel<int>> SubmitGrievanceAsync(int clientId, int memberId, SubmitGrievanceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            string ticketNo = $"GRV-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}";
            const string sql = @"
                INSERT INTO dbo.GRIEVANCES (CLIENT_ID, MEMBER_ID, TICKET_NO, SUBJECT, DESCRIPTION, CATEGORY, PRIORITY, STATUS)
                OUTPUT INSERTED.GRIEVANCE_ID
                VALUES (@ClientId, @MemberId, @TicketNo, @Subject, @Description, @Category, @Priority, N'OPEN')";

            int id = await connection.QuerySingleAsync<int>(new CommandDefinition(
                sql,
                new { ClientId = clientId, MemberId = memberId, TicketNo = ticketNo, request.Subject, request.Description, request.Category, request.Priority },
                cancellationToken: cancellationToken));

            return new ResponseModel<int> { Data = id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitGrievanceAsync failed.");
            return new ResponseModel<int> { ErrorMessage = "Unable to submit grievance.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<IReadOnlyList<GrievanceListItemDto>>> GetMyGrievancesAsync(int clientId, int memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                SELECT GRIEVANCE_ID AS GrievanceId, TICKET_NO AS TicketNo, SUBJECT AS Subject, ISNULL(CATEGORY, N'') AS Category,
                       ISNULL(PRIORITY, N'') AS Priority, ISNULL(STATUS, N'') AS Status, SUBMITTED_AT AS SubmittedAt,
                       RESOLVED_AT AS ResolvedAt, ADMIN_RESPONSE AS AdminResponse
                FROM dbo.GRIEVANCES
                WHERE CLIENT_ID = @ClientId AND MEMBER_ID = @MemberId
                ORDER BY SUBMITTED_AT DESC";

            IEnumerable<GrievanceListItemDto> rows = await connection.QueryAsync<GrievanceListItemDto>(
                new CommandDefinition(sql, new { ClientId = clientId, MemberId = memberId }, cancellationToken: cancellationToken));

            return new ResponseModel<IReadOnlyList<GrievanceListItemDto>> { Data = rows.ToList() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyGrievancesAsync failed.");
            return new ResponseModel<IReadOnlyList<GrievanceListItemDto>> { ErrorMessage = "Unable to load grievances.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<PagedResponse<GrievanceListItemDto>>> GetGrievancesAsync(int clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                ;WITH q AS (
                    SELECT GRIEVANCE_ID AS GrievanceId, TICKET_NO AS TicketNo, SUBJECT AS Subject, ISNULL(CATEGORY, N'') AS Category,
                           ISNULL(PRIORITY, N'') AS Priority, ISNULL(STATUS, N'') AS Status, SUBMITTED_AT AS SubmittedAt,
                           RESOLVED_AT AS ResolvedAt, ADMIN_RESPONSE AS AdminResponse, COUNT(*) OVER() AS Total
                    FROM dbo.GRIEVANCES WHERE CLIENT_ID = @ClientId
                )
                SELECT * FROM q ORDER BY SubmittedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            IEnumerable<dynamic> rows = await connection.QueryAsync(new CommandDefinition(
                sql, new { ClientId = clientId, Offset = (page - 1) * pageSize, PageSize = pageSize }, cancellationToken: cancellationToken));

            List<dynamic> list = rows.ToList();
            int total = list.FirstOrDefault()?.Total ?? 0;
            List<GrievanceListItemDto> records = list.Select(r => new GrievanceListItemDto
            {
                GrievanceId = (int)r.GrievanceId,
                TicketNo = (string)r.TicketNo,
                Subject = (string)r.Subject,
                Category = (string)r.Category,
                Priority = (string)r.Priority,
                Status = (string)r.Status,
                SubmittedAt = (DateTime)r.SubmittedAt,
                ResolvedAt = r.ResolvedAt as DateTime?,
                AdminResponse = r.AdminResponse as string
            }).ToList();

            return new ResponseModel<PagedResponse<GrievanceListItemDto>>
            {
                Data = new PagedResponse<GrievanceListItemDto> { Total = total, Page = page, PageSize = pageSize, Records = records }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetGrievancesAsync failed.");
            return new ResponseModel<PagedResponse<GrievanceListItemDto>> { ErrorMessage = "Unable to load grievances.", ErrorId = -1 };
        }
    }

    public async Task<ResponseModel<bool>> UpdateGrievanceAsync(int clientId, int grievanceId, UpdateGrievanceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            IDbConnection connection = _connectionFactory.GetConnection();
            const string sql = @"
                UPDATE dbo.GRIEVANCES
                SET STATUS = @Status, ADMIN_RESPONSE = @AdminResponse,
                    RESOLVED_AT = CASE WHEN @Status IN (N'RESOLVED', N'CLOSED') THEN SYSUTCDATETIME() ELSE RESOLVED_AT END
                WHERE GRIEVANCE_ID = @GrievanceId AND CLIENT_ID = @ClientId";

            int affected = await connection.ExecuteAsync(new CommandDefinition(
                sql, new { GrievanceId = grievanceId, ClientId = clientId, request.Status, request.AdminResponse },
                cancellationToken: cancellationToken));

            return new ResponseModel<bool> { Data = affected > 0 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateGrievanceAsync failed.");
            return new ResponseModel<bool> { ErrorMessage = "Unable to update grievance.", ErrorId = -1 };
        }
    }
}
