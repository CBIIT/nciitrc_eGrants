namespace eGrants.Models
{
    /// <summary>
    /// The email rule model.
    /// </summary>
    public class EmailRule
    {
        /// <summary>
        /// Gets or sets the pa.
        /// </summary>
        public string? pa { get; set; }

        /// <summary>
        /// Gets or sets the email_to.
        /// </summary>
        public string? email_to { get; set; }

        /// <summary>
        /// Gets or sets the email_cc.
        /// </summary>
        public string? email_cc { get; set; }

        /// <summary>
        /// Gets or sets the start_date.
        /// </summary>
        public string? start_date { get; set; }

        /// <summary>
        /// Gets or sets the end_date.
        /// </summary>
        public string? end_date { get; set; }

        /// <summary>
        /// Gets or sets the email_template_id.
        /// </summary>
        public string? email_template_id { get; set; }

        /// <summary>
        /// Gets or sets the email_template_name.
        /// </summary>
        public string? email_template_name { get; set; }

        /// <summary>
        /// Gets or sets the email_body.
        /// </summary>
        public string? email_body { get; set; }

        /// <summary>
        /// Gets or sets the email_subject.
        /// </summary>
        public string? email_subject { get; set; }

        /// <summary>
        /// Gets or sets the person_name.
        /// </summary>
        public string? person_name { get; set; }
    }
}