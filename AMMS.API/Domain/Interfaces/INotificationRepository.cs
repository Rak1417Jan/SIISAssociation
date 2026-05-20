using MVEA.Domain.Entities;
using MVEA.Domain.Enums;

namespace MVEA.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByMLAIdAsync(int mlaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetScheduledNotificationsAsync(DateTime? beforeDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type, int mlaId, CancellationToken cancellationToken = default);
}
