using System.ComponentModel.DataAnnotations;

namespace PLM.api.Models.DTO
{
    public class RegisterRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]        
        public string FullName { get; set; }
        [Required]       
        public string Password { get; set; }
    }
}
