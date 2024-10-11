using AutoMapper;
using AutoMapper.QueryableExtensions;
using Lets_Connect.Data.DTO;
using Lets_Connect.Helpers;
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

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = context.Users.AsQueryable();

            query = query.Where(x => x.UserName != userParams.CurrentUserName);

            if(userParams.Gender != null)
            {
                query = query.Where(x => x.Gender == userParams.Gender);
            }

            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(x => x.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive),
            };
            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider),
                                                           userParams.PageNumber, userParams.PageSize);
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

        public void Update(User user)
        {
            context.Entry(user).State = EntityState.Modified;
        }
    }
}
