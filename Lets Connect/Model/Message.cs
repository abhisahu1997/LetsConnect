namespace Lets_Connect.Model
{
    public class Message
    {
        public int Id { get; set; }
        public required string SenderUserName { get; set; }
        public required string ReceiverUserName { get; set; }
        public required string Context {  get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        public bool SenderDeleted { get; set; }
        public bool ReceipientDeleted { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public int RecepientId { get; set; }
        public User Recepient { get; set; } = null!;
    }
}
