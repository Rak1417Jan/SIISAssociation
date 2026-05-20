using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Role service implementation with Unit of Work pattern
/// </summary>
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ILogger<RoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = new List<RoleResponse>
        {
            new RoleResponse
            {
                Id = (int)UserRole.SystemAdmin,
                Role = UserRole.SystemAdmin,
                Name = "System Admin",
                Description = "System administrator with full access to all features"
            },
            new RoleResponse
            {
                Id = (int)UserRole.MLA,
                Role = UserRole.MLA,
                Name = "MLA",
                Description = "Member of Legislative Assembly - primary owner"
            },
            new RoleResponse
            {
                Id = (int)UserRole.MLATeamMember,
                Role = UserRole.MLATeamMember,
                Name = "MLA Team Member",
                Description = "MLA team member with limited permissions for chat, tickets, and content"
            },
            new RoleResponse
            {
                Id = (int)UserRole.Voter,
                Role = UserRole.Voter,
                Name = "Voter",
                Description = "End user - voter of assembly constituency"
            }
        };

        return await Task.FromResult(roles);
    }

    public async Task<bool> AssignRoleAsync(AssignRoleRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");
            }

            // Update user role
            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);

            // TODO: Handle role-specific entity creation/updates
            // For example, if changing from Voter to MLA, need to create MLA entity
            // If changing from MLA to Voter, need to handle MLA entity cleanup

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Role {Role} assigned to user {UserId}", request.Role, request.UserId);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
