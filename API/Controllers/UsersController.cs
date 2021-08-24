using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext context;

        public UsersController(DataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers(){
           return  await this.context.Users.ToListAsync();
            
        }

        
        [HttpGet("{id}")]
         public async Task<ActionResult<AppUser>> GetUsers(int id){
            return await this.context.Users.FindAsync(id);
           
        }
    }
}