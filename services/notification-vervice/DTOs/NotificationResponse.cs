namespace notification_service.DTOs
{
    public class NotificationResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; }
    }
}