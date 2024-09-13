using Lets_Connect.Data.DTO;
using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface IUserRepository
    {
        void Update(User user);

        Task<bool> SaveAllAsync();
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User?> GetUSerByIdAsync(int id);
        Task<User?> GetUSerByNameAsync(string name);
        Task<MemberDto?> GetMemberAsync(string username);
        Task<IEnumerable<MemberDto?>> GetMembersAsync();
    }
}
