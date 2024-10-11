using Lets_Connect.Data.DTO;
using Lets_Connect.Helpers;
using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface IUserRepository
    {
        void Update(User user);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUSerByIdAsync(int id);
        Task<User?> GetUSerByNameAsync(string name);
        Task<MemberDto?> GetMemberAsync(string username);
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    }
}
