using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string UserName)
        {
            return await this.context.Users.Where(x => x.UserName == UserName)
            .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
           .SingleOrDefaultAsync();
        }

        public async Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = this.context.Users
            .AsQueryable();

            query = query.Where(user => user.UserName != userParams.CurrentUserName);
            query = query.Where(user => user.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(user => user.DateOfBirth >= minDob && user.DateOfBirth <= maxDob);
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(user => user.Created),
                _ => query.OrderByDescending(user => user.LastActive)
            };
            return await PageList<MemberDto>
                  .CreateAsync(query.ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                              .AsNoTracking(),
                               userParams.PageNumber, userParams.PageSize);

        }

        public async Task<AppUser> GetUserByIdAsync(int Id)
        {
            return await this.context.Users.FindAsync(Id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string UserName)
        {
            return await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(user => user.UserName == UserName);

        }

        public async Task<string> GetUserGender(string username)
        {
            return await this.context.Users
               .Where(x=>x.UserName==username)
               .Select(x=>x.Gender)
               .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await this.context.Users.Include(p => p.Photos).ToListAsync();

        }

      

        public void Update(AppUser user)
        {
            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}