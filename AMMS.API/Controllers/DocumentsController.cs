using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Platform;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
public sealed class DocumentsController : AmmsControllerBase
{
    private readonly IPlatformService _platformService;

    public DocumentsController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpPost("{applicationId:int}/upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ResponseModel<DocumentUploadResponse>>> Upload(
        [FromRoute] int applicationId,
        [FromForm] string documentType,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<DocumentUploadResponse> result = await _platformService.UploadDocumentAsync(clientId, applicationId, documentType, file, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{applicationId:int}")]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<DocumentListItemDto>>>> List([FromRoute] int applicationId, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<IReadOnlyList<DocumentListItemDto>> result = await _platformService.GetDocumentsAsync(clientId, applicationId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{documentId:int}/ai-verify")]
    public async Task<ActionResult<ResponseModel<DocumentAiVerifyResponse>>> AiVerify([FromRoute] int documentId, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<DocumentAiVerifyResponse> result = await _platformService.AiVerifyDocumentAsync(clientId, documentId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = "MinRole:Manager")]
    [HttpPatch("{documentId:int}/verify")]
    public async Task<ActionResult<ResponseModel<bool>>> Verify([FromRoute] int documentId, [FromBody] VerifyDocumentRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetClientId(out int clientId))
        {
            return Unauthorized();
        }

        ResponseModel<bool> result = await _platformService.VerifyDocumentAsync(clientId, documentId, request, cancellationToken);
        return Ok(result);
    }
}
