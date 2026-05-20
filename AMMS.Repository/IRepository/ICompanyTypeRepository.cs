using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.IRepository;

public interface ICompanyTypeRepository
{
    Task<ResponseModel<IReadOnlyList<CompanyTypeResponse>>> GetCompanyTypesAsync(CancellationToken cancellationToken = default);
}
