namespace eGrants.Models
{
    public class IMPP_Admin_Supplements_WIP
    {
        public int? Serial_num { get; set; }
	    public int? Supp_appl_id { get; set; }
	    public string? Full_grant_num { get; set; }
	    public int? Former_num { get; set; }
	    public int? Former_appl_id { get; set; }
	    public DateTime Submitted_date { get; set; }
	    public int? movedto_appl_id { get; set; }
	    public int? Support_year { get; set; }
        public string? Suffix_code { get; set; }
        public string? file_type { get; set; }
        public int? category_id { get; set;}
	    public string? url { get; set; }
        public DateTime Created_date { get; set;}
	    public int? moved_by { get; set;}
	    public DateTime moved_date { get; set;}
	    public int? adm_supp_wip_id { get; set; }
        public string? movedto_document_id { get; set; }
        public string? sub_category_name { get; set; }
        public string? doc_url { get; set; }
        public int? ACCESSION_NUMBER { get; set; }
    }
}
