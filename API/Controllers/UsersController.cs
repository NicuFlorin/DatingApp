using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTO;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetMembersAsync();
            return Ok(users);

        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUsers(string username)
        {
            return await userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {

            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            this.mapper.Map(memberUpdateDto, user);
            this.userRepository.Update(user);

            if (await this.userRepository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var result = await this.photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await this.userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { Username = user.username }, this.mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo!");
            }
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }
            photo.IsMain = true;

            if (await this.userRepository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Failed to set main photo!");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null)
            {
                return NotFound();
            }
            if (photo.IsMain) return BadRequest("You can not delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }

            }
            user.Photos.Remove(photo);

            if (await this.userRepository.SaveAllAsync())
            {
                return Ok();
            }
            else return BadRequest("Failed to delete your photo");

        }
    }
}