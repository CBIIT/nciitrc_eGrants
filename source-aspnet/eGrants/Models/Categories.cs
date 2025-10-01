namespace eGrants.Models
{
    public class Categories
    {
        /// <summary>
        ///     Gets or sets the category_id.
        /// </summary>
        public int category_id { get; set; }

        /// <summary>
        ///     Gets or sets the category_name.
        /// </summary>
        public string category_name { get; set; }

        /// <summary>
        ///     Gets or sets the package.
        /// </summary>
        public string package { get; set; }

        /// <summary>
        ///     Gets or sets the input_type.
        /// </summary>
        public string input_type { get; set; }

        /// <summary>
        ///     Gets or sets the input_constraint.
        /// </summary>
        public string input_constraint { get; set; }
        public DateTime created_date { get; set; }
        public int created_by_person_id { get; set; }
        public string impac_doc_type_code { get; set; }
        public string modified_date { get; set; }
        public int modified_by_person_id { get; set; }

        public string can_upload { get; set; }

    }
}