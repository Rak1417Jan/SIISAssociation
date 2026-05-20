using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// User service implementation with Unit of Work pattern
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByMobileAsync(request.MobileNumber, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with mobile number {request.MobileNumber} already exists");
            }

            // Check email if provided
            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingEmail != null)
                {
                    throw new InvalidOperationException($"User with email {request.Email} already exists");
                }
            }

            // Hash password if provided
            string? passwordHash = null;
            if (!string.IsNullOrEmpty(request.Password))
            {
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            // Create user entity
            var user = new User
            {
                MobileNumber = request.MobileNumber,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = request.Role,
                IsActive = true,
                IsEmailVerified = false,
                IsMobileVerified = false,
                IsTwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdUser = await _userRepository.AddAsync(user, cancellationToken);

            // TODO: Create related entities based on role (Voter, MLATeamMember, etc.)
            // This would be done in separate repositories/services

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToUserResponse(createdUser);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found");
        }

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                // Check if email already exists
                var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingEmail != null && existingEmail.Id != id)
                {
                    throw new InvalidOperationException($"Email {request.Email} is already in use");
                }
                user.Email = request.Email;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                // Name would be stored in related entity (Voter, MLA, etc.)
                // For now, we'll just update if there's a Name field in User entity
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            if (request.IsTwoFactorEnabled.HasValue)
            {
                user.IsTwoFactorEnabled = request.IsTwoFactorEnabled.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;
            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToUserResponse(user);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            _userRepository.SoftDelete(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            MobileNumber = user.MobileNumber,
            Email = user.Email,
            Role = user.Role,
            RoleName = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsMobileVerified = user.IsMobileVerified,
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }
}
