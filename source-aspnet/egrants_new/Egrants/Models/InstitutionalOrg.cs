namespace egrants_new.Egrants.Models
{

 public class InstitutionalOrg
        {
            public int Tag { get; set; }
            public int OrgId { get; set; }
            public string Org_name { get; set; }
            public string created_by { get; set; }
            public string created_date { get; set; }
            public string end_date { get; set; }
            public string sv_url { get; set; }
        }
}