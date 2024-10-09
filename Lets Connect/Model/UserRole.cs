using Microsoft.AspNetCore.Identity;

namespace Lets_Connect.Model
{
    public class UserRole: IdentityUserRole<int>
    {
        public User User { get; set; } = null!;
        public Roles Role { get; set; } = null!;
    }
}
