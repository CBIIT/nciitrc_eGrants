namespace egrants_new.Egrants.Models
{

 public class InstitutionalOrg
        {
            public int Tag { get; set; }
            public int OrgId { get; set; }
            public string OrgName { get; set; }
            public string CreatedBy { get; set; }
            public string CreatedDate { get; set; }
            public string EndDate { get; set; }
            public string SvUrl { get; set; }
            public bool Active { get; set; }
        }
}