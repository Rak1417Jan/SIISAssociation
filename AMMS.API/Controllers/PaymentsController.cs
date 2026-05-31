using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public sealed class PaymentsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public PaymentsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost("create-order")]
    public async Task<ActionResult<ResponseModel<CreatePaymentOrderResponse>>> CreateOrder([FromBody] CreatePaymentOrderRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<CreatePaymentOrderResponse> result = await _platformService.CreatePaymentOrderAsync(clientId, memberId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ResponseModel<VerifyPaymentResponse>>> Verify([FromBody] VerifyPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<VerifyPaymentResponse> result = await _platformService.VerifyPaymentAsync(clientId, memberId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<ResponseModel<PagedResponse<PaymentHistoryItemDto>>>> History([FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<PagedResponse<PaymentHistoryItemDto>> result = await _platformService.GetPaymentHistoryAsync(clientId, memberId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ResponseModel<PaymentSummaryDto>>> Summary(CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<PaymentSummaryDto> result = await _platformService.GetPaymentSummaryAsync(clientId, memberId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("renewal")]
    public async Task<ActionResult<ResponseModel<CreatePaymentOrderResponse>>> Renewal([FromBody] RenewalPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId) || !TryGetMemberId(out int memberId))
        {
            return Unauthorized();
        }

        ResponseModel<CreatePaymentOrderResponse> result = await _platformService.CreateRenewalOrderAsync(clientId, memberId, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Finance")]
    [HttpPost("refund")]
    public async Task<ActionResult<ResponseModel<RefundPaymentResponse>>> Refund([FromBody] RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<RefundPaymentResponse> result = await _platformService.RefundPaymentAsync(clientId, request, cancellationToken);
        return Ok(result);
    }
}
