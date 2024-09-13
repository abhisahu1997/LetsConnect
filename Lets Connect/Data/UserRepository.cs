using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lets_Connect.Data.DTO;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.EntityFrameworkCore;

namespace Lets_Connect.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public UserRepository(DataContext context, IMapper mapper) 
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto?> GetMemberAsync(string username)
        {
            return await context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDto?>> GetMembersAsync()
        {
            return await context.Users.ProjectTo<MemberDto?>(mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<User?> GetUSerByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<User?> GetUSerByNameAsync(string name)
        {
            return await context.Users.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == name);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await context.Users.Include(x => x.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Update(User user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
