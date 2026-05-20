using MVEA.Comman;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.IRepository;
using MVEA.Services.IService;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MVEA.Services.Service;

public sealed class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITokenDenylistRepository _tokenDenylistRepository;

    public StaffService(IStaffRepository staffRepository, ITokenDenylistRepository tokenDenylistRepository)
    {
        _staffRepository = staffRepository;
        _tokenDenylistRepository = tokenDenylistRepository;
    }

    public Task<ResponseModel<IReadOnlyList<StaffListItemResponse>>> GetStaffAsync(int clientId, CancellationToken cancellationToken = default)
        => _staffRepository.GetStaffAsync(clientId, cancellationToken);

    public async Task<ResponseModel<int>> CreateStaffAsync(int clientId, CreateStaffRequest request, int createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email)
            || request.RoleIds == null || request.RoleIds.Count == 0 || request.RoleIds.Any(id => id <= 0))
        {
            return new ResponseModel<int> { ErrorMessage = "username, email, and at least one valid roleIds entry are required.", ErrorId = -1 };
        }

        bool usedProvidedPassword = !string.IsNullOrWhiteSpace(request.Password);
        string plainForHash = usedProvidedPassword
            ? request.Password!.Trim()
            : GenerateTempPassword(12);

        if (usedProvidedPassword && plainForHash.Length < 8)
        {
            return new ResponseModel<int> { ErrorMessage = "password must be at least 8 characters.", ErrorId = -1 };
        }

        CommandMethods.PasswordHashResult hashResult = CommandMethods.ConvertToHashResult(plainForHash);

        var create = await _staffRepository.CreateStaffAsync(
            clientId,
            request,
            hashResult.PasswordHash,
            hashResult.PasswordSalt,
            createdBy,
            cancellationToken);

        if (!create.Success)
        {
            return new ResponseModel<int> { ErrorMessage = create.ErrorMessage, ErrorId = create.ErrorId };
        }

        return new ResponseModel<int> { Data = create.Data };
    }

    public async Task<ResponseModel<bool>> UpdateStaffAsync(int clientId, int id, UpdateStaffRequest request, int modifiedBy, CancellationToken cancellationToken = default)
    {
        var result = await _staffRepository.UpdateStaffAsync(clientId, id, request, modifiedBy, cancellationToken);
        if (result.Success && request.RoleIds != null && request.RoleIds.Count > 0)
        {
            await _tokenDenylistRepository.AddAsync(id, "*", "RoleChanged", cancellationToken);
        }
        return result;
    }

    public async Task<ResponseModel<bool>> DeactivateStaffAsync(int clientId, int id, int modifiedBy, CancellationToken cancellationToken = default)
    {
        var result = await _staffRepository.DeactivateStaffAsync(clientId, id, modifiedBy, cancellationToken);
        if (result.Success)
        {
            await _tokenDenylistRepository.AddAsync(id, "*", "Deactivated", cancellationToken);
        }
        return result;
    }

    private static string GenerateTempPassword(int length)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append(alphabet[b % alphabet.Length]);
        }
        return sb.ToString();
    }
}
