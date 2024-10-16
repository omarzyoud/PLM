namespace PLM.api.Models.DTO
{
    public class GetMediaDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Convert enum to string
        public int UploaderId { get; set; }
        public int? ApprovedById { get; set; }
    }
}
