namespace egrants_new.Egrants.Models
{

    public class InstitutionalOrg
    {
        public int Tag { get; set; }
        public int OrgId { get; set; }
        public string OrgName { get; set; }
        public string SVCreatedBy { get; set; }
        public string SVCreatedDate { get; set; }

        public string SVEndDate { get; set; }
        public string SvUrl { get; set; }
        public string FUCreatedDate { get; set; }
        public string FUCreatedBy { get; set; }
        public string FUEndDate { get; set; }
        public string FUUrl { get; set; }
        public string ODCreatedBy { get; set; }
        public string ODCreatedDate { get; set; }
        public string ODEndDate { get; set; }
        public string ODUrl { get; set; }
        public bool Active { get; set; }
    }
}