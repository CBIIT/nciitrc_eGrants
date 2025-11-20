namespace eGrants.Models
{
    public class Egrants
    {
        public int document_id { get; set; }
        public string? admin_phs_org_code { get; set; }
        public int? serial_num { get; set; }
        public string? full_grant_num { get; set; }
        public int? appl_id { get; set; }
        public short? category_id { get; set; }
        public string? sub_category_name { get; set; }
        public string? document_name { get; set; }
        public DateTime? document_date { get; set; }
        public DateTime? qc_date { get; set; }
        public int? parent_id { get; set; }
        public string qc_userid { get; set; }
        public string url { get; set; }
        public string created_by { get; set; }
        public DateTime? created_date { get; set; }
    }
}
