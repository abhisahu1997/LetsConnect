using AutoMapper;
using Lets_Connect.Data;
using Lets_Connect.Data.DTO;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Lets_Connect.Controllers
{
    
    public class AccountController : BaseApiController
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        public AccountController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if( await UserExists(registerDto.UserName)) return BadRequest("UserName already exists");

            var user = mapper.Map<User>(registerDto);

            user.UserName = registerDto.UserName.ToLower();
            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.ToString());
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,

            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == userName.ToUpper());
        }

        [HttpPost("login")]

        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.Users
                .Include(y => y.Photos).
                FirstOrDefaultAsync(x => x.NormalizedUserName == loginDto.UserName.ToUpper());

            if (user == null || user.UserName == null) return Unauthorized("Invalid username or password");
            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if(!result) return Unauthorized();


            return new UserDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Token = await tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                Gender = user.Gender,

            };
        }
    }
}
