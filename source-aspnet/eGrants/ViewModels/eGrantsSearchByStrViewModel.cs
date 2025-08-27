using eGrants.Models;

namespace eGrants.ViewModels
{
    public class eGrantsSearchByStrViewModel
    {
        public int tag { get; set; }
        //public int parent { get; set; }

        public int grant_id { get; set; }
        //public string? label { get; set; }
        //public string? serial_num { get; set; }
        //public string? admin_phs_org_code { get; set; }

        public string? former_grant_num { get; set; }
        public string? latest_full_grant_num { get; set; }
        public string? all_activity_code { get; set; }

        public string? project_title { get; set; }
        //public int? org_id { get; set; }
        public string? org_name { get; set; }
        //public string? pi_name { get; set; }

        public string? current_pi_name { get; set; }
        public string? current_pi_email_address { get; set; }
        public string? current_pd_name { get; set; }
        public string? current_pd_email_address { get; set; }
        public string? current_spec_name { get; set; }
        public string? current_spec_email_address { get; set; }
        public string? current_bo_email_address { get; set; }
        public string? prog_class_code { get; set; }
        public string? sv_url { get; set; }
        //public string? arra_flag { get; set; }
        public string? fda_flag { get; set; }
        public string? stop_flag { get; set; }
        public string? ms_flag { get; set; }
        public string? od_flag { get; set; }
        public string? ds_flag { get; set; }
        public int? adm_supp { get; set; }
        public bool? institutional_flag1 { get; set; }
        //public int? institutional_flag2 { get; set; }
        //public string? inst_flag1_url { get; set; }
        //public int? appl_id { get; set; }
        //ublic string? full_grant_num { get; set; }
        //public byte support_year { get; set; }
        //public string? project_title_2 { get; set; }
        //public string? appl_type_code { get; set; }
        //public string? deleted_by_impac { get; set; }
        //public int? doc_count { get; set; }
        //public int? closeout_notcount { get; set; }
        //public string? competing { get; set; }
        //public int? fsr_count { get; set; }
        //public int? frc_destroyed { get; set; }
        //public string? appl_fda_flag { get; set; }
        //public string? appl_ms_flag { get; set; }
        //public string? appl_od_flag { get; set; }
        //public string? appl_ds_flag { get; set; }
        //public string? closeout_flag { get; set; }
        //public string? irppr_id { get; set; }
        //public string? can_add_doc { get; set; }
        //public string? can_add_funding { get; set; }

        //public int? docs_count { get; set; }
        //public int? is_current_pi { get; set; }

        public string? specific_year_pi_name { get; set; }
        //public string? specific_year_pi_email_address { get; set; }
        //public string? specific_year_project_name { get; set; }
        //public string? specific_year_org_name { get; set; }
        //public string? specific_year_full_grant_num { get; set; }
        ////had to add these as special properties to hold selected values for details view
        public string? FullGrantNumber { get; set; }
        public string? grant_num { get; set; }
        public string? SelectedProjectName { get; set; }
        public string? SelectedGrantPiName { get; set; }
        public string? SelectedOrganizationName { get; set; }

        public string? SelectedGrantPiEmail { get; set; }
        public List<PersonContact> MPIContacts { get; set; }

        public int? OrgId { get; set; }

        public bool? AnyOrgDoc { get; set; }
    }
}
