namespace notification_service.DTOs
{
    public class NotificationRequest
    {
        public string Receiver { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }
    }
}