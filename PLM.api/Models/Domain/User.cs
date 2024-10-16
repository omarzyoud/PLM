using System.ComponentModel.DataAnnotations;

namespace PLM.api.Models.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public UserType Type { get; set; } // Enum for Admin or User

        // Navigation properties
        public ICollection<Media> UploadedMedia { get; set; }
        public ICollection<Media> ApprovedMedia { get; set; }
    }
    public enum UserType
    {
        Admin,
        User
    }
}
