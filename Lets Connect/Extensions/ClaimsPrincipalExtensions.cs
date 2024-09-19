using System.Security.Claims;

namespace Lets_Connect.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var username = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (username == null) throw new Exception("Cannot get username from token");
            return username;
        }
    }
}
