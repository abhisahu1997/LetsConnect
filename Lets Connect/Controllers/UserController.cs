using AutoMapper;
using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Helpers;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lets_Connect.Controllers
{
    [Authorize]
    public class UserController : BaseApiController 
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        public UserController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;   
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            userParams.CurrentUserName = User.GetUserName();
            var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            
            Response.AddPaginationHeader(users);

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await unitOfWork.UserRepository.GetMemberAsync(username);

            if (user == null) return NotFound();

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
         
            var user = await unitOfWork.UserRepository.GetUSerByNameAsync(User.GetUserName());

            if (user == null) return BadRequest("Could not find the user");

            mapper.Map(memberUpdateDto, user);

            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to update the user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await unitOfWork.UserRepository.GetUSerByNameAsync(User.GetUserName());

            if (user == null) return BadRequest("Could not find the user");

            var result = await photoService.AddPhotoAsync(file);
            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await unitOfWork.Complete()) return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-profile-picture/{photoId:int}")]
        public async Task<ActionResult> SetProfilePicture(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUSerByNameAsync(User.GetUserName());
            if (user == null) return BadRequest("Could not find the user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("Cannot use this as Profile Picture");

            var currentPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentPhoto != null) currentPhoto.IsMain = false;
            photo.IsMain = true;

            if(await unitOfWork.Complete()) return NoContent();

            return BadRequest("Unable to set the profile picture");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await unitOfWork.UserRepository.GetUSerByNameAsync(User.GetUserName());
            if (user == null) return BadRequest("Could not find the user");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);

                user.Photos.Remove(photo);
            }
            if(await unitOfWork.Complete()) { return NoContent(); }

            return BadRequest("Problem Deleting Photos");
        }
    }
}
