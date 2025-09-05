namespace eGrants.Models
{
    public class Grants
    {
        public int grant_id { get; set; }
        public string admin_phs_org_code { get; set; }
        public int serial_num { get; set; }
        public string mechanism_code { get; set; }
        public string close_out_date { get; set; }
        public string former_admin_phs_org_code { get; set; }
        public int former_serial_num { get; set; }
        public string future_admin_phs_org_code { get; set; }
        public int future_serial_num { get; set; }
        public string stop_sign { get; set; }
        public string paperless { get; set; }
        public string is_tobacco { get; set; }
        public string grant_close_date { get; set; }
        public string to_be_destroyed { get; set; }
        public string destruction_reason { get; set; }
        public string arra_flag { get; set; }
        public string fda_flag { get; set; }
    }
}
