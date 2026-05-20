using MVEA.Application.DTOs.Request;
using MVEA.Application.DTOs.Response;
using MVEA.Application.Interfaces;
using MVEA.Domain.Entities;
using MVEA.Domain.Enums;
using MVEA.Domain.Interfaces;

namespace MVEA.Application.Services;

/// <summary>
/// Notification service implementation with Unit of Work pattern
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDeliveryRepository _notificationDeliveryRepository;
    private readonly IMLARepository _mlaRepository;
    private readonly IVoterRepository? _voterRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        INotificationRepository notificationRepository,
        INotificationDeliveryRepository notificationDeliveryRepository,
        IMLARepository mlaRepository,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
        _notificationDeliveryRepository = notificationDeliveryRepository;
        _mlaRepository = mlaRepository;
        _logger = logger;
    }

    public async Task<ScheduledNotificationResponse> ScheduleNotificationAsync(ScheduleNotificationRequest request, int mlaId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Verify MLA exists
            var mla = await _mlaRepository.GetByIdAsync(mlaId, cancellationToken);
            if (mla == null)
            {
                throw new KeyNotFoundException($"MLA with ID {mlaId} not found");
            }

            // Create notification
            var notification = new Notification
            {
                MLAId = mlaId,
                Type = request.Type,
                Title = request.Title,
                MessageTemplate = request.MessageTemplate,
                ScheduledDate = request.ScheduledDate ?? DateTime.UtcNow,
                DeliveryChannel = request.DeliveryChannel,
                IsSent = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdNotification = await _notificationRepository.AddAsync(notification, cancellationToken);

            // TODO: Queue notification delivery to background service
            // This would typically involve:
            // 1. Fetching recipient list (voters) based on AssemblyId/BoothId
            // 2. Processing template placeholders (VoterName, Age, BoothNo, etc.)
            // 3. Sending via InApp/WhatsApp/SMS channels
            // 4. Creating NotificationDelivery records

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Notification scheduled for MLA {MLAId}, Type: {Type}, Scheduled: {ScheduledDate}", 
                mlaId, request.Type, notification.ScheduledDate);

            return MapToScheduledNotificationResponse(createdNotification);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationTemplateResponse>> GetNotificationTemplatesAsync(CancellationToken cancellationToken = default)
    {
        // Return predefined templates
        var templates = new List<NotificationTemplateResponse>
        {
            new NotificationTemplateResponse
            {
                Id = 1,
                Type = NotificationType.Birthday,
                TypeName = "Birthday",
                Title = "Birthday Greeting",
                MessageTemplate = "Dear {VoterName}, Wish you a very Happy Birthday! May this special day bring you joy and happiness. - {MLAName}",
                Description = "Template for birthday greetings to voters",
                Placeholders = new List<string> { "{VoterName}", "{MLAName}", "{Age}" }
            },
            new NotificationTemplateResponse
            {
                Id = 2,
                Type = NotificationType.Anniversary,
                TypeName = "Anniversary",
                Title = "Anniversary Greeting",
                MessageTemplate = "Dear {VoterName}, Wishing you and your family a very Happy Anniversary! May you continue to create beautiful memories together. - {MLAName}",
                Description = "Template for anniversary greetings to voters",
                Placeholders = new List<string> { "{VoterName}", "{MLAName}" }
            },
            new NotificationTemplateResponse
            {
                Id = 3,
                Type = NotificationType.Festival,
                TypeName = "Festival",
                Title = "Festival Greeting",
                MessageTemplate = "Dear {VoterName}, Wishing you and your family a very Happy {FestivalName}! May this festival bring prosperity and happiness to your home. - {MLAName}",
                Description = "Template for festival greetings",
                Placeholders = new List<string> { "{VoterName}", "{MLAName}", "{FestivalName}" }
            },
            new NotificationTemplateResponse
            {
                Id = 4,
                Type = NotificationType.GovernmentScheme,
                TypeName = "Government Scheme",
                Title = "Government Scheme Announcement",
                MessageTemplate = "Dear {VoterName}, Important update: {SchemeName} is now available. Visit {SchemeLink} for more details. - {MLAName}",
                Description = "Template for government scheme announcements",
                Placeholders = new List<string> { "{VoterName}", "{MLAName}", "{SchemeName}", "{SchemeLink}" }
            },
            new NotificationTemplateResponse
            {
                Id = 5,
                Type = NotificationType.General,
                TypeName = "General",
                Title = "General Message",
                MessageTemplate = "Dear {VoterName}, {MessageContent}. - {MLAName}",
                Description = "General purpose template",
                Placeholders = new List<string> { "{VoterName}", "{MLAName}", "{MessageContent}" }
            }
        };

        return await Task.FromResult(templates);
    }

    public async Task<IEnumerable<NotificationDeliveryLogResponse>> GetDeliveryLogsAsync(
        int? notificationId = null,
        int? voterId = null,
        bool? isDelivered = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        // Get delivery logs
        var deliveries = await _notificationDeliveryRepository.GetDeliveryLogsAsync(
            notificationId, voterId, isDelivered, cancellationToken);

        // Apply date filter if provided
        if (startDate.HasValue || endDate.HasValue)
        {
            deliveries = deliveries.Where(d =>
                (!startDate.HasValue || d.CreatedAt >= startDate.Value) &&
                (!endDate.HasValue || d.CreatedAt <= endDate.Value));
        }

        // Apply pagination
        var paginatedDeliveries = deliveries
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var logResponses = new List<NotificationDeliveryLogResponse>();

        foreach (var delivery in paginatedDeliveries)
        {
            // Get notification details
            var notification = await _notificationRepository.GetByIdAsync(delivery.NotificationId, cancellationToken);

            logResponses.Add(new NotificationDeliveryLogResponse
            {
                Id = delivery.Id,
                NotificationId = delivery.NotificationId,
                NotificationTitle = notification?.Title ?? string.Empty,
                NotificationType = notification?.Type ?? NotificationType.General,
                NotificationTypeName = notification?.Type.ToString() ?? "General",
                VoterId = delivery.VoterId,
                RecipientName = delivery.RecipientName,
                RecipientMobile = delivery.RecipientMobile,
                DeliveredMessage = delivery.DeliveredMessage,
                IsDelivered = delivery.IsDelivered,
                DeliveredAt = delivery.DeliveredAt,
                DeliveryError = delivery.DeliveryError,
                ExternalMessageId = delivery.ExternalMessageId,
                DeliveryChannel = notification?.DeliveryChannel.ToString() ?? string.Empty,
                CreatedAt = delivery.CreatedAt
            });
        }

        return logResponses;
    }

    private ScheduledNotificationResponse MapToScheduledNotificationResponse(Notification notification)
    {
        return new ScheduledNotificationResponse
        {
            Id = notification.Id,
            MLAId = notification.MLAId,
            Type = notification.Type,
            TypeName = notification.Type.ToString(),
            Title = notification.Title,
            MessageTemplate = notification.MessageTemplate,
            ScheduledDate = notification.ScheduledDate,
            IsSent = notification.IsSent,
            SentAt = notification.SentAt,
            DeliveryChannel = notification.DeliveryChannel,
            DeliveryChannelName = notification.DeliveryChannel.ToString(),
            TotalRecipients = 0, // Will be updated when delivery is processed
            DeliveredCount = 0,
            FailedCount = 0,
            CreatedAt = notification.CreatedAt
        };
    }
}
