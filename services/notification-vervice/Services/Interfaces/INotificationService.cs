using notification_service.DTOs;

namespace notification_service.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> Send(NotificationRequest request);
    }
}