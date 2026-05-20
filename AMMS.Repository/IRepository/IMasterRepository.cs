using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;

namespace MVEA.Repository.Interfaces;

public interface IMasterRepository 
{
    Task<ResponseModel<IList<MasterResponse>>> GetMasterAsync(MasterRequest request, CancellationToken cancellationToken = default);


}
