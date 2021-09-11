using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Helpers;

namespace API.Extensions
{
    public interface IUserRepository
    {
        void Update(AppUser user);


        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int Id);

        Task<AppUser> GetUserByUsernameAsync(string UserName);

        Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams);

        Task<MemberDto> GetMemberAsync(string UserName);
        Task<string> GetUserGender(string username);

    }
}