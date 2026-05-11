namespace booking_service.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int MKH { get; set; }

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Message>? Messages { get; set; }
    }
}