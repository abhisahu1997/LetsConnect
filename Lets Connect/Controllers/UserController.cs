using AutoMapper;
using Lets_Connect.Data.DTO;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
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

        //[HttpGet("{id}")]
        //public async Task<ActionResult<User>> GetUser(int id)
        //{
        //    var user = await userRepository.GetUSerByIdAsync(id);

        //    if(user == null) return NotFound();

        //    return user;
        //}

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string name)
        {
            var user = await userRepository.GetMemberAsync(name);

            if (user == null) return NotFound();

            return user;
        }
    }
}
