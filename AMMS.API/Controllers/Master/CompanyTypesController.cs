using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVEA.Model.DTOs.Response;
using MVEA.Services.IService;

namespace MVEA.API.Controllers.Master;

[ApiController]
[Route("api/v1/master/company-types")]
public sealed class CompanyTypesController : ControllerBase
{
    private readonly ICompanyTypeService _companyTypeService;

    public CompanyTypesController(ICompanyTypeService companyTypeService)
    {
        _companyTypeService = companyTypeService;
    }

    /// <summary>Lists all company type options (lookup for firms).</summary>
    [Authorize(Policy = "MinRole:Manager")]
    [HttpGet]
    public async Task<ActionResult<ResponseModel<IReadOnlyList<CompanyTypeResponse>>>> GetCompanyTypes(CancellationToken cancellationToken)
    {
        return Ok(await _companyTypeService.GetCompanyTypesAsync(cancellationToken));
    }
}
