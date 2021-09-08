using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTO
{

    public class RegisterDto
    {

        [Required]
        public string UserName { get; set; }
        [Required]
        public string KnownAS { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string password { get; set; }



    }

}