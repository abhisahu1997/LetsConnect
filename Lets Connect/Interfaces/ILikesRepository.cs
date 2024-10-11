using Lets_Connect.Data.DTO;
using Lets_Connect.Helpers;
using Lets_Connect.Model;

namespace Lets_Connect.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike?> GetUserLike(int sourceUserID, int targetUserID);
        Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);
        Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
        void DeleteLike(UserLike like);
        void AddLike(UserLike like);
    }
}
