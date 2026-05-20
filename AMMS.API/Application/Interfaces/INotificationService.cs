using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;

namespace MVEA.Application.Interfaces;

public interface INotificationService
{
    Task<ScheduledNotificationResponse> ScheduleNotificationAsync(ScheduleNotificationRequest request, int mlaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplateResponse>> GetNotificationTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationDeliveryLogResponse>> GetDeliveryLogsAsync(
        int? notificationId = null,
        int? voterId = null,
        bool? isDelivered = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
