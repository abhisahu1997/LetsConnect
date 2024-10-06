using Lets_Connect.Model;

namespace Lets_Connect.Data.DTO
{
    public class MessageDto
    {
        public int Id { get; set; }
        public required string SenderUserName { get; set; }
        public required string SenderPhotoUrl { get; set; }
        public int RecepientId { get; set; }
        public required string ReceipientUserName { get; set; }
        public required string RecepientPhotoUrl { get; set; }
        public required string Context { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } 
    }
}
