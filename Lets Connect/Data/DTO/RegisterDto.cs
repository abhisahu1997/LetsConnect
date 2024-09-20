namespace Lets_Connect.Data.DTO
{
    public class RegisterDto
    {
        public required string UserName { get; set; }
        public required string KnownAs { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public required string DateOfBirth { get; set; }

        public required string Password { get; set; }
    }
}
