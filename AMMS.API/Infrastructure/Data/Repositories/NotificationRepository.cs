using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Notification repository implementation using Dapper
/// </summary>
public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    protected override string TableName => "Notifications";

    public NotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<Notification>> GetByMLAIdAsync(int mlaId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Notifications WHERE MLAId = @MLAId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Notification>(
            new CommandDefinition(query, new { MLAId = mlaId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Notification>> GetScheduledNotificationsAsync(DateTime? beforeDate = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Notifications WHERE IsSent = 0 AND IsDeleted = 0";
        
        if (beforeDate.HasValue)
        {
            query += " AND (ScheduledDate IS NULL OR ScheduledDate <= @BeforeDate)";
        }
        else
        {
            query += " AND (ScheduledDate IS NULL OR ScheduledDate <= @Now)";
            beforeDate = DateTime.UtcNow;
        }

        query += " ORDER BY ScheduledDate ASC";

        return await _connection.QueryAsync<Notification>(
            new CommandDefinition(query, new { BeforeDate = beforeDate, Now = DateTime.UtcNow }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type, int mlaId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM Notifications WHERE MLAId = @MLAId AND Type = @Type AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<Notification>(
            new CommandDefinition(query, new { MLAId = mlaId, Type = (int)type }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "MLAId, Type, Title, MessageTemplate, ScheduledDate, IsSent, SentAt, DeliveryChannel, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@MLAId, @Type, @Title, @MessageTemplate, @ScheduledDate, @IsSent, @SentAt, @DeliveryChannel, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "MLAId = @MLAId, Type = @Type, Title = @Title, MessageTemplate = @MessageTemplate, ScheduledDate = @ScheduledDate, IsSent = @IsSent, SentAt = @SentAt, DeliveryChannel = @DeliveryChannel, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
