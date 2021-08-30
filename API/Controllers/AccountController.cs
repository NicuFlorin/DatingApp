using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.username))
                return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                username = registerDto.username,
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                passwordSalt = hmac.Key

            };
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDto
            {
                username = user.username,
                token = this.tokenService.createToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.context.Users.Include(p => p.Photos).SingleOrDefaultAsync(user => user.username == loginDto.username);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            using var hmac = new HMACSHA512(user.passwordSalt);

            var computerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));

            for (int i = 0; i < computerHash.Length; i++)
            {
                if (computerHash[i] != user.passwordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return new UserDto
            {
                username = user.username,
                token = this.tokenService.createToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(user => user.username.ToLower() == username.ToLower());
        }
    }
}