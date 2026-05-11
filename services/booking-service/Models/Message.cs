namespace booking_service.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string Role { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public Conversation Conversation { get; set; }
    }
}