using eGrants.Models;

namespace eGrants.ViewModels
{
    public class eGrantsDocUploadViewModel
    {
        public int? DocId { get; set; }
        public int? ApplId { get; set; }
        public string? DocName { get; set; }
        public string? DocDate { get; set; }
        public string? FullGrantNum { get; set; }
    }
}
