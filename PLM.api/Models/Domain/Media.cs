using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PLM.api.Models.Domain
{
    public class Media
    {
        [Key]
        public int MediaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public MediaStatus Status { get; set; } // Enum for Pending, Rejected, or Accepted

        [Required]
        public byte[] FileData { get; set; } // Stores file data

        // Foreign key and navigation properties
        [Required]
        [ForeignKey("Uploader")]
        public int UploaderId { get; set; }
        public User Uploader { get; set; }

        [ForeignKey("ApprovedBy")]
        public int? ApprovedById { get; set; } // Nullable since it may not be approved yet
        public User ApprovedBy { get; set; }
    }
    public enum MediaStatus
    {
        Pending=1,
        Rejected=2,
        Accepted=3
    }
}
