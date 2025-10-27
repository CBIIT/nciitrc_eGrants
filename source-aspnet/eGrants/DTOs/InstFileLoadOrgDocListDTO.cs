using System.ComponentModel.DataAnnotations.Schema;

namespace eGrants.DTOs
{
    public class InstFileLoadOrgDocListDTO
    {
        [Column("org_id")]
        public int org_id { get; set; }
        [Column("Org_Name")]
        public string? OrgName { get; set; }
        [Column("document_id")]
        public int DocumentId { get; set; }
        public string? category_name { get; set; }
        public string? url { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string? created_date { get; set; }
        public string? comments { get; set; }
    }
}
