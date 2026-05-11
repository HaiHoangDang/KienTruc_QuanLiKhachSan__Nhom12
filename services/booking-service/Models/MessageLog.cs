namespace booking_service.Models
{
    public class MessageLog
    {
        public int Id { get; set; }

        public int? MessageId { get; set; }

        public int MKH { get; set; }

        public int ConversationId { get; set; }

        public string Role { get; set; }

        public string Content { get; set; }

        public DateTime LoggedAt { get; set; }
    }
}
