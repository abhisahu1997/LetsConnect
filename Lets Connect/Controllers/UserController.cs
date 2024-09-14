using Lets_Connect.Data.DTO;
using Lets_Connect.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lets_Connect.Controllers
{
    [Authorize]
    public class UserController : BaseApiController 
    {
        private readonly IUserRepository userRepository;
        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);

            if (user == null) return NotFound();

            return user;
        }
    }
}
