namespace API.Entities
{
    public class AppUser
    {
        public int id { get; set; }

        public string username { get; set; }

        public byte[] passwordHash {get;set;}

        public byte[] passwordSalt { get; set; }


        
    }
} 