namespace eGrants.Models
{
    public class VwCategories
    {

        public int category_id { get; set; }
        public string category_name { get; set; }
        public string? package { get; set; }
        public string? ic { get; set; }
        public int removed_by_person_id { get; set; }
        public string? input_type { get; set; }
        public string? input_constraint { get; set; }
        public DateTime removed_date { get; set; }
        public int added_by_person_id { get; set; }
        public DateTime added_date { get; set; }
        public string can_upload { get; set; }
        public string impac_doc_type_code { get; set; }
    }
}
