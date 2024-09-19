using AutoMapper;
using Azure.Identity;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lets_Connect.Controllers
{
    [Authorize]
    public class UserController : BaseApiController 
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        public UserController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;   
            this.photoService = photoService;
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

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
         
            var user = await userRepository.GetUSerByNameAsync(User.GetUserName());

            if (user == null) return BadRequest("Could not find the user");

            mapper.Map(memberUpdateDto, user);

            if (await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUSerByNameAsync(User.GetUserName());

            if (user == null) return BadRequest("Could not find the user");

            var result = await photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            user.Photos.Add(photo);

            if (await userRepository.SaveAllAsync()) return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-profile-picture/{photoId:int}")]
        public async Task<ActionResult> SetProfilePicture(int photoId)
        {
            var user = await userRepository.GetUSerByNameAsync(User.GetUserName());
            if (user == null) return BadRequest("Could not find the user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("Cannot use this as Profile Picture");

            var currentPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentPhoto != null) currentPhoto.IsMain = false;
            photo.IsMain = true;

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Unable to set the profile picture");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUSerByNameAsync(User.GetUserName());
            if (user == null) return BadRequest("Could not find the user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);

                user.Photos.Remove(photo);
            }
            if(await userRepository.SaveAllAsync()) { return NoContent(); }

            return BadRequest("Problem Deleting Photos");
        }
    }
}
