using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}
