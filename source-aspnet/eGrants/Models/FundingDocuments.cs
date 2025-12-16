namespace eGrants.Models
{
    /// <summary>
    /// Represents a funding document
    /// </summary>
    public class FundingDocuments
    {
        public string serial_num { get; set; }
        public string admin_code { get; set; }
        public string appl_id { get; set; }
        public string full_grant_num { get; set; }
        public string document_id { get; set; }
        public string doc_label { get; set; }
        public string url { get; set; }
        public string category_id { get; set; }
        public string category_name { get; set; }
        public string document_fy { get; set; }
        public string created_date { get; set; }
        public string arra_flag { get; set; }
    }
}