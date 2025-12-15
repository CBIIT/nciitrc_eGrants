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
        public string? SVCreatedBy { get; set; }
        public string? SVCreatedDate { get; set; }
        public string? SVEndDate { get; set; }
        public string? SvUrl { get; set; }
        public string? FUCreatedBy { get; set; }
        public string? FUCreatedDate { get; set; }
        public string? FUEndDate { get; set; }
        public string? FUUrl { get; set; }
        public string? AnyOrgDoc { get; set; }  
    }
}
