
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Services.Interfaces;

public interface IMasterService
{
    Task<ResponseModel<IList<MasterResponse>>> GetMasterAsync(MasterRequest request, CancellationToken cancellationToken = default);


}
