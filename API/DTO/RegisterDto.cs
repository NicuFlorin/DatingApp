using System.ComponentModel.DataAnnotations;

namespace API.DTO{

   public class RegisterDto{

       [Required]
       public string username { get; set; }
       [Required]
       public string password { get; set; }
   }
 
}