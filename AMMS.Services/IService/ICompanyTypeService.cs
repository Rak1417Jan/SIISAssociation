using MVEA.Model.DTOs.Response;

namespace MVEA.Services.IService;

public interface ICompanyTypeService
{
    Task<ResponseModel<IReadOnlyList<CompanyTypeResponse>>> GetCompanyTypesAsync(CancellationToken cancellationToken = default);
}
