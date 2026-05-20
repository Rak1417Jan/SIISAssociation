using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface IVoterService
{
    Task<VoterVerificationResponse> VerifyVoterAsync(VerifyVoterRequest request, CancellationToken cancellationToken = default);
    Task<VoterProfileResponse> GetVoterProfileAsync(int voterId, CancellationToken cancellationToken = default);
    Task<FamilyMemberResponse> AddFamilyMemberAsync(AddFamilyMemberRequest request, int voterId, CancellationToken cancellationToken = default);
}
