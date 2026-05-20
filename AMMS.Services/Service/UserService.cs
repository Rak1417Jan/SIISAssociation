using Microsoft.Extensions.Logging;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.UnitOfWork;
using MVEA.Services.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// User service implementation with Unit of Work pattern
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    

    public UserService(
        IUnitOfWork unitOfWork,    
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;        
        _logger = logger;
    }

    public async Task<ResponseModel<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.UserRepository.CreateUserAsync(request, cancellationToken);
        }
        catch
        {
           return new ResponseModel<UserResponse>
            {
                Data = null,
                ErrorMessage= "An error occurred while creating the user",
           };
        }
    }

    //public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    //{
    //    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    //    if (user == null)
    //    {
    //        throw new KeyNotFoundException($"User with ID {id} not found");
    //    }

    //    return MapToUserResponse(user);
    //}

    //public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    //{
    //    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    //    try
    //    {
    //        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    //        if (user == null)
    //        {
    //            throw new KeyNotFoundException($"User with ID {id} not found");
    //        }

    //        // Update fields if provided
    //        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
    //        {
    //            // Check if email already exists
    //            var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
    //            if (existingEmail != null && existingEmail.Id != id)
    //            {
    //                throw new InvalidOperationException($"Email {request.Email} is already in use");
    //            }
    //            user.Email = request.Email;
    //        }

    //        if (!string.IsNullOrEmpty(request.Name))
    //        {
    //            // Name would be stored in related entity (Voter, MLA, etc.)
    //            // For now, we'll just update if there's a Name field in User entity
    //        }

    //        if (request.IsActive.HasValue)
    //        {
    //            user.IsActive = request.IsActive.Value;
    //        }

    //        if (request.IsTwoFactorEnabled.HasValue)
    //        {
    //            user.IsTwoFactorEnabled = request.IsTwoFactorEnabled.Value;
    //        }

    //        user.UpdatedAt = DateTime.UtcNow;
    //        _userRepository.Update(user);

    //        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //        await _unitOfWork.CommitTransactionAsync(cancellationToken);

    //        return MapToUserResponse(user);
    //    }
    //    catch
    //    {
    //        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    //        throw;
    //    }
    //}

    //public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    //{
    //    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    //    try
    //    {
    //        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    //        if (user == null)
    //        {
    //            throw new KeyNotFoundException($"User with ID {id} not found");
    //        }

    //        _userRepository.SoftDelete(user);

    //        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //        await _unitOfWork.CommitTransactionAsync(cancellationToken);

    //        return true;
    //    }
    //    catch
    //    {
    //        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    //        throw;
    //    }
    //}

    //private UserResponse MapToUserResponse(User user)
    //{
    //    return new UserResponse
    //    {
    //        Id = user.Id,
    //        MobileNumber = user.MobileNumber,
    //        Email = user.Email,
    //        Role = user.Role,
    //        RoleName = user.Role.ToString(),
    //        IsActive = user.IsActive,
    //        IsEmailVerified = user.IsEmailVerified,
    //        IsMobileVerified = user.IsMobileVerified,
    //        IsTwoFactorEnabled = user.IsTwoFactorEnabled,
    //        LastLoginAt = user.LastLoginAt,
    //        CreatedAt = user.CreatedAt
    //    };
    //}
}
