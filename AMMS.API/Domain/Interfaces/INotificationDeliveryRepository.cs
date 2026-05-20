using MVEA.Domain.Entities;

namespace MVEA.Domain.Interfaces;

public interface INotificationDeliveryRepository : IRepository<NotificationDelivery>
{
    Task<IEnumerable<NotificationDelivery>> GetByNotificationIdAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationDelivery>> GetDeliveryLogsAsync(int? notificationId = null, int? voterId = null, bool? isDelivered = null, CancellationToken cancellationToken = default);
    Task<NotificationDelivery?> GetByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default);
}
