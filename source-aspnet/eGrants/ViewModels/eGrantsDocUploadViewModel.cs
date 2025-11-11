using eGrants.Models;

namespace eGrants.ViewModels
{
    public class eGrantsDocUploadViewModel
    {
        public int? DocId { get; set; }
        public int? ApplId { get; set; }
        public string? DocName { get; set; }
        public DateTime? DocDate { get; set; }
        public string? FullGrantNum { get; set; }
    }
}
