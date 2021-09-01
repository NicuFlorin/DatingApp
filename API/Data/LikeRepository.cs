using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikeRepository : ILikesRepository
    {
        private readonly DataContext context;
        public LikeRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await this.context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PageList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = this.context.Users.OrderBy(user => user.username).AsQueryable();

            var likes = this.context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }
            else if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }
            else return null;
            var likeUsers = users.Select(user => new LikeDto
            {
                Username = user.username,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.id
            });

            return await PageList<LikeDto>.CreateAsync(likeUsers,likesParams.PageNumber,likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await this.context.Users
               .Include(user => user.LikedUsers)
               .FirstOrDefaultAsync(user => user.id == userId);
        }
    }
}