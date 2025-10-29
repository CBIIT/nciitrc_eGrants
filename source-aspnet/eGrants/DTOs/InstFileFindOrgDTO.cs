using System.ComponentModel.DataAnnotations.Schema;

namespace eGrants.DTOs
{
    public class InstFileFindOrgDTO
    {
        [Column("org_id")]
        public int OrgId { get; set; }
        [Column("Org_Name")]
        public string? OrgName { get; set; }
        public int index_id { get; set; }
        public string? created_by { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? sv_url { get; set; }
    }
}
