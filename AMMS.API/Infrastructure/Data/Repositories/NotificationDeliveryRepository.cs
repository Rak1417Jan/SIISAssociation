using System.Data;
using Dapper;
using MVEA.Domain.Entities;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.Infrastructure.Data.Repositories;

/// <summary>
/// Notification delivery repository implementation using Dapper
/// </summary>
public class NotificationDeliveryRepository : BaseRepository<NotificationDelivery>, INotificationDeliveryRepository
{
    protected override string TableName => "NotificationDeliveries";

    public NotificationDeliveryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<NotificationDelivery>> GetByNotificationIdAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM NotificationDeliveries WHERE NotificationId = @NotificationId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await _connection.QueryAsync<NotificationDelivery>(
            new CommandDefinition(query, new { NotificationId = notificationId }, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<NotificationDelivery>> GetDeliveryLogsAsync(int? notificationId = null, int? voterId = null, bool? isDelivered = null, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM NotificationDeliveries WHERE IsDeleted = 0";
        var parameters = new DynamicParameters();

        if (notificationId.HasValue)
        {
            query += " AND NotificationId = @NotificationId";
            parameters.Add("NotificationId", notificationId.Value);
        }

        if (voterId.HasValue)
        {
            query += " AND VoterId = @VoterId";
            parameters.Add("VoterId", voterId.Value);
        }

        if (isDelivered.HasValue)
        {
            query += " AND IsDelivered = @IsDelivered";
            parameters.Add("IsDelivered", isDelivered.Value);
        }

        query += " ORDER BY CreatedAt DESC";

        return await _connection.QueryAsync<NotificationDelivery>(
            new CommandDefinition(query, parameters, _transaction, cancellationToken: cancellationToken));
    }

    public async Task<NotificationDelivery?> GetByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM NotificationDeliveries WHERE ExternalMessageId = @ExternalMessageId AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<NotificationDelivery>(
            new CommandDefinition(query, new { ExternalMessageId = externalMessageId }, _transaction, cancellationToken: cancellationToken));
    }

    protected override string GetInsertColumns()
    {
        return "NotificationId, VoterId, RecipientMobile, RecipientName, DeliveredMessage, IsDelivered, DeliveredAt, DeliveryError, ExternalMessageId, CreatedAt, IsDeleted, CreatedBy";
    }

    protected override string GetInsertValues()
    {
        return "@NotificationId, @VoterId, @RecipientMobile, @RecipientName, @DeliveredMessage, @IsDelivered, @DeliveredAt, @DeliveryError, @ExternalMessageId, @CreatedAt, @IsDeleted, @CreatedBy";
    }

    protected override string GetUpdateSetClause()
    {
        return "NotificationId = @NotificationId, VoterId = @VoterId, RecipientMobile = @RecipientMobile, RecipientName = @RecipientName, DeliveredMessage = @DeliveredMessage, IsDelivered = @IsDelivered, DeliveredAt = @DeliveredAt, DeliveryError = @DeliveryError, ExternalMessageId = @ExternalMessageId, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy";
    }
}
