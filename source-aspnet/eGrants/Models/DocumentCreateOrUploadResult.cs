namespace eGrants.Models
{
    public class DocumentCreateOrUploadResult()
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? DocumentId { get; set; }
        public string? Url { get; set; }
    }
}
