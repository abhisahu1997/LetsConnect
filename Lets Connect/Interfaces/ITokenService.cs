using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
