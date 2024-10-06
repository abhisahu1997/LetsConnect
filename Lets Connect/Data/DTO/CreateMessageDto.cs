namespace Lets_Connect.Data.DTO
{
    public class CreateMessageDto
    {
        public required string RecepientUserName { get; set; }
        public required string Content { get; set; }
    }
}
