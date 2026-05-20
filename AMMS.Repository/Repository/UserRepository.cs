using Dapper;
using Microsoft.Extensions.Logging;

using MVEA.Comman;
using MVEA.Model.DTOs.Request;
using MVEA.Model.DTOs.Response;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.Interfaces;

using System.Data;


namespace MVEA.Repository.Repositories;

/// <summary>
/// User repository implementation using Dapper
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connection;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ISqlConnectionFactory connection, ILogger<UserRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }


    public async Task<ResponseModel<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            _logger.LogWarning("CreateUserAsync called with null request.");
            return new ResponseModel<UserResponse>() { ErrorMessage = "Request cannot be null.", ErrorId = -1 };
        }

        if (string.IsNullOrWhiteSpace(request.MobileNumber) && string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("CreateUserAsync called without MobileNumber and Email.");
            return new ResponseModel<UserResponse>() { ErrorMessage = "Either MobileNumber or Email must be provided.", ErrorId = -1 };
        }

        var connection = _connection.GetConnection();
        if (connection == null)
        {
            _logger.LogError("Database connection is not available in CreateUserAsync.");
            return new ResponseModel<UserResponse>() { ErrorMessage = "Database connection is not available.", ErrorId = -1 };
        }


        try
        {
            var hashResult = CommandMethods.ConvertToHashResult(request.Password!);

            // Map request values to stored procedure parameters
            var spParams = new
            {
                UserId = (int?)null, // null => insert. If you want update logic, set from request.UserId
                ClientId = request.GetType().GetProperty("ClientId") != null ? (int?)(int)request.GetType().GetProperty("ClientId")!.GetValue(request)! : null,
                Username = request.UserName ,
                PasswordHash = (object?)hashResult.PasswordHash ?? DBNull.Value,
                PasswordSalt = (object?)hashResult.PasswordSalt ?? DBNull.Value,
                FullName = request.GetType().GetProperty("Name") != null ? request.GetType().GetProperty("Name")!.GetValue(request) : request.GetType().GetProperty("FullName")?.GetValue(request),
                EmailId = request.Email,
                MobileNo = request.MobileNumber,
                Role = request.Role.ToString(),
                IsActive = true,
                ModifiedBy = request.MobileNumber ?? request.Email
            };

            // CALL STORED PROCEDURE
            var cmd = new CommandDefinition(
                "sp_InsertOrUpdateUser",
                spParams,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            // Stored proc returns inserted/updated id. Use QuerySingleAsync<int>.
            var returnedId = await connection.QueryAsync<UserResponse>(cmd);

            return new ResponseModel<UserResponse>() { Data = returnedId.FirstOrDefault() };
        }
        catch (OperationCanceledException)
        {            
            _logger.LogInformation("CreateUserAsync cancelled.");
            return new ResponseModel<UserResponse>() { ErrorMessage = "CreateUserAsync cancelled.", ErrorId = -1 };
        }
        catch (Exception ex)
        {            
            _logger.LogError(ex, "Error while creating/updating user via stored procedure.");
            return new ResponseModel<UserResponse>() { ErrorMessage = "Error while creating/updating user.", ErrorId = -1 };
        }
    }

    //public async Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken cancellationToken = default)
    //{
    //    var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
    //    return await _connection.QueryFirstOrDefaultAsync<User>(
    //        new CommandDefinition(query, new { MobileNumber = mobileNumber }, _transaction, cancellationToken: cancellationToken));
    //}

    //public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrEmpty(email))
    //        return null;

    //    var query = "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = 0";
    //    return await _connection.QueryFirstOrDefaultAsync<User>(
    //        new CommandDefinition(query, new { Email = email }, _transaction, cancellationToken: cancellationToken));
    //}

    //public async Task<IEnumerable<User>> GetByRoleAsync(int role, CancellationToken cancellationToken = default)
    //{
    //    var query = "SELECT * FROM Users WHERE Role = @Role AND IsDeleted = 0";
    //    return await _connection.QueryAsync<User>(
    //        new CommandDefinition(query, new { Role = role }, _transaction, cancellationToken: cancellationToken));
    //}

    //protected override string GetInsertColumns()
    //{
    //    return "MobileNumber, Email, PasswordHash, Role, IsActive, IsEmailVerified, IsMobileVerified, IsTwoFactorEnabled, CreatedAt, IsDeleted, CreatedBy";
    //}

    //protected override string GetInsertValues()
    //{
    //    return "@MobileNumber, @Email, @PasswordHash, @Role, @IsActive, @IsEmailVerified, @IsMobileVerified, @IsTwoFactorEnabled, @CreatedAt, @IsDeleted, @CreatedBy";
    //}

    //protected override string GetUpdateSetClause()
    //{
    //    return "Email = @Email, PasswordHash = @PasswordHash, IsActive = @IsActive, IsEmailVerified = @IsEmailVerified, IsMobileVerified = @IsMobileVerified, IsTwoFactorEnabled = @IsTwoFactorEnabled, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    //}
}
