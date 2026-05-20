using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;

namespace MVEA.Services.Service;

public sealed class CompanyTypeService : ICompanyTypeService
{
    private readonly ICompanyTypeRepository _companyTypeRepository;

    public CompanyTypeService(ICompanyTypeRepository companyTypeRepository)
    {
        _companyTypeRepository = companyTypeRepository;
    }

    public Task<ResponseModel<IReadOnlyList<CompanyTypeResponse>>> GetCompanyTypesAsync(CancellationToken cancellationToken = default)
        => _companyTypeRepository.GetCompanyTypesAsync(cancellationToken);
}
