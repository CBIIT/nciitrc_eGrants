namespace eGrants.Models
{
    /// <summary>
    /// The email templates model.
    /// </summary>
    public class EmailTemplates
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string? id { get; set; }

        /// <summary>
        /// Gets or sets the template_name.
        /// </summary>
        public string? template_name { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        public string? body { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string? subject { get; set; }

        /// <summary>
        /// Gets or sets the created_date.
        /// </summary>
        public string? created_date { get; set; }

        /// <summary>
        /// Gets or sets the created_by_person_id.
        /// </summary>
        public string? created_by_person_id { get; set; }
    }
}