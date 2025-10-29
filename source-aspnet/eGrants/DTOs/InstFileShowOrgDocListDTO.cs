using System.ComponentModel.DataAnnotations.Schema;

namespace eGrants.DTOs
{
    public class InstFileShowOrgDocListDTO
    {
        [Column("org_id")]
        public int OrgId { get; set; }
        [Column("Org_Name")]
        public string? OrgName { get; set; }
        public int index_id { get; set; }
        [Column("svcreated_by")]
        public string? SVCreatedBy { get; set; }
        [Column("svcreated_date")]
        public DateTime? SVCreatedDate { get; set; }
        [Column("svend_date")]
        public DateTime? SVEndDate { get; set; }
        [Column("sv_url")]
        public string? SvUrl { get; set; }
        [Column("fucreated_by")]
        public string? FUCreatedBy { get; set; }
        [Column("fucreated_date")]
        public DateTime? FUCreatedDate { get; set; }
        [Column("fuend_date")]
        public DateTime? FUEndDate { get; set; }
        public string? fu_url { get; set; }
        public string? anyorgdoc { get; set; }

    }
}
