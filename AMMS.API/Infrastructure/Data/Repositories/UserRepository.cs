using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// User repository implementation using Dapper
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    protected override string TableName => "Users";

    public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<User?> GetByMobileAsync(string mobileNumber, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(query, new { MobileNumber = mobileNumber }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var query = "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            new CommandDefinition(query, new { Email = email }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(int role, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Users WHERE Role = @Role AND IsDeleted = 0";
        return await _connection.QueryAsync<User>(
            new CommandDefinition(query, new { Role = role }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "MobileNumber, Email, PasswordHash, Role, IsActive, IsEmailVerified, IsMobileVerified, IsTwoFactorEnabled, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@MobileNumber, @Email, @PasswordHash, @Role, @IsActive, @IsEmailVerified, @IsMobileVerified, @IsTwoFactorEnabled, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "Email = @Email, PasswordHash = @PasswordHash, IsActive = @IsActive, IsEmailVerified = @IsEmailVerified, IsMobileVerified = @IsMobileVerified, IsTwoFactorEnabled = @IsTwoFactorEnabled, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
