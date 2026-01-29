using Finitech.Modules.Notifications.Contracts.DTOs;

namespace Finitech.Modules.Notifications.Contracts;

public interface INotificationsService
{
    Task SendNotificationAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
    Task SendTemplatedNotificationAsync(SendTemplatedNotificationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(Guid partyId, int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task UpdatePreferenceAsync(UpdateNotificationPreferenceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(Guid partyId, CancellationToken cancellationToken = default);
}
