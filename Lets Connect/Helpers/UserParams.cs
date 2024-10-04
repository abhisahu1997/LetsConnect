namespace Lets_Connect.Helpers
{
    public class UserParams: PaginationParams
    {
        public string? Gender { get; set; }
        public string? CurrentUserName { get; set; }
        public int MinAge { get; set; } = 18;
        public string OrderBy { get; set; } = "lastActive";
    }
}
