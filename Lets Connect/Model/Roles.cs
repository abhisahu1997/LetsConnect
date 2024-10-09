using Microsoft.AspNetCore.Identity;

namespace Lets_Connect.Model
{
    public class Roles: IdentityRole<int>
    {
        public ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
