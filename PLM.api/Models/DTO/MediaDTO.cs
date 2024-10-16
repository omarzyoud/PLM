using System.ComponentModel.DataAnnotations;

namespace PLM.api.Models.DTO
{
    public class MediaDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public IFormFile fildata { get; set; }


    }
}
