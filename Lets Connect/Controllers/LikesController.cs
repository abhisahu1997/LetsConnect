using Lets_Connect.Data.DTO;
using Lets_Connect.Extensions;
using Lets_Connect.Helpers;
using Lets_Connect.Interfaces;
using Lets_Connect.Model;
using Microsoft.AspNetCore.Mvc;

namespace Lets_Connect.Controllers
{
    public class LikesController(IUnitOfWork unitOfWork): BaseApiController
    {
        [HttpPost("{targetUserId:int}")]
        public async Task<ActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            var existingLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);
            if (existingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId,
                };
                unitOfWork.LikesRepository.AddLike(like);
            }
            else
            {
                unitOfWork.LikesRepository.DeleteLike(existingLike);
            }

            if (await unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to update likes");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users);
            return Ok(users);
        }
    }
}
