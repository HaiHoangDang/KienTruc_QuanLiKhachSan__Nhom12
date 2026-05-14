using notification_service.DTOs;
using notification_service.Services.Interfaces;

namespace notification_service.Services
{
    public class NotificationService : INotificationService
    {
        public Task<NotificationResponse> Send(NotificationRequest request)
        {
            if (request == null)
            {
                throw new Exception("Dữ liệu thông báo không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(request.Receiver))
            {
                throw new Exception("Người nhận không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                throw new Exception("Nội dung thông báo không được để trống.");
            }

            Console.WriteLine("===== NOTIFICATION =====");
            Console.WriteLine("Receiver: " + request.Receiver);
            Console.WriteLine("Title: " + request.Title);
            Console.WriteLine("Type: " + request.Type);
            Console.WriteLine("Message: " + request.Message);
            Console.WriteLine("========================");

            var response = new NotificationResponse
            {
                Success = true,
                Message = "Gửi thông báo thành công.",
                SentAt = DateTime.Now
            };

            return Task.FromResult(response);
        }
    }
}