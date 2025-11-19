namespace eGrants.DTOs
{
    public class CategoriesListDTO
    {
        /// <summary>
        ///     Gets or sets the category_id.
        /// </summary>
        public int category_id { get; set; }

        /// <summary>
        ///     Gets or sets the category_name.
        /// </summary>
        public string category_name { get; set; }
        public string? package { get; set; }
        public string? input_type { get; set; }
        public string? input_constraint { get; set; }
    }
}
