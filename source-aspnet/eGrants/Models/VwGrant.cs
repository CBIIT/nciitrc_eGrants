namespace eGrants.Models
{
    public class VwGrant
    {
        public int? grant_id { get; set; }
        public string? admin_phs_org_code { get; set; }
        public int? serial_num { get; set; }
        public string? mechanism_code { get; set; }
        public string? grant_num { get; set; }
        public string? former_admin_phs_org_code { get; set; }
        public int? former_serial_num { get; set; }
        public string? former_grant_num { get; set; }
        public string? future_admin_phs_org_code { get; set; }
        public int? future_serial_num { get; set; }
        public string? paperless { get; set; }
        public int? person_id { get; set; }
        public string? future_grant_num { get; set; }
        public string? org_name { get; set; }
        public string? project_title { get; set; }
        public string? active_grant_flag { get; set; }
        public int? fy { get; set; }
        public string? prog_class_code { get; set; }
        public string? last_name { get; set; }
        public string? first_name { get; set; }
        public string? mi_name { get; set; }
        public string? pi_name { get; set; }
        public bool? is_tobacco { get; set; }
        public bool? to_be_destroyed { get; set; }
        public string? closed_out { get; set; }
        public string? stop_sign { get; set; }
        public string? paper_file { get; set; }
        public string? award_package { get; set; }
        public string? application_package { get; set; }
        public string? correspondence_package { get; set; }
        public string? closeout_package { get; set; }
        public bool? is_funded { get; set; }
        public string? grant_close_date { get; set; }
        public string? org_sv_url { get; set; }
        public int? adm_supp { get; set; }
        public string? current_pd_name { get; set; }
        public string? current_pd_email_address { get; set; }
        public string? current_pi_name { get; set; }
        public string? current_pi_email_address { get; set; }
        public string? current_spec_name { get; set; }
        public string? current_spec_email_address { get; set; }
        public string? current_bo_email_address { get; set; }
        public string? MS_flag { get; set; }
        public string? OD_flag { get; set; }
        public string? STP_flag { get; set; }
        public string? FDA_flag { get; set; }
        public string? ARRA_flag { get; set; }
        public string? DS_flag { get; set; }
        public int? Institutional_flag1 { get; set; }
        public int? Institutional_flag2 { get; set; }
        public string? inst_flag1_url { get; set; }
    }
}
