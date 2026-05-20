using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.UnitOfWork;
using MVEA.Services.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// User service implementation with Unit of Work pattern
/// </summary>
public class MasterService : IMasterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MasterService> _logger;
    

    public MasterService(
        IUnitOfWork unitOfWork,    
        ILogger<MasterService> logger)
    {
        _unitOfWork = unitOfWork;        
        _logger = logger;
    }

    public async Task<ResponseModel<IList<MasterResponse>>> GetMasterAsync(MasterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.MasterRepository.GetMasterAsync(request, cancellationToken);
        }
        catch
        {
           return new ResponseModel<IList<MasterResponse>>
            {
                Data = null,
                ErrorMessage= "An error occurred while getting the master",
           };
        }
    }

    
}
