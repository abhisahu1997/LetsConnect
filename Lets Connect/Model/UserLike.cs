namespace Lets_Connect.Model
{
    public class UserLike
    {
        public User SourceUser { get; set; } = null!;
        public int SourceUserId { get; set; }
        public User TargetUser { get; set; } = null!;
        public int TargetUserId { get; set; }
    }
}
