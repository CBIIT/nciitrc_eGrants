namespace eGrants.Models
{
    /// <summary>
    /// The email status model.
    /// </summary>
    public class EmailStatus
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string? id { get; set; }

        /// <summary>
        /// Gets or sets the email_type.
        /// </summary>
        public string? email_type { get; set; }

        /// <summary>
        /// Gets or sets the email_address.
        /// </summary>
        public string? email_address { get; set; }

        /// <summary>
        /// Gets or sets the email_date.
        /// </summary>
        public string? email_date { get; set; }

        /// <summary>
        /// Gets or sets the email_send_status.
        /// </summary>
        public string? email_send_status { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public string? position { get; set; }

        /// <summary>
        /// Gets or sets the person_name.
        /// </summary>
        public string? person_name { get; set; }

        /// <summary>
        /// Gets or sets the created_date.
        /// </summary>
        public string? created_date { get; set; }

        /// <summary>
        /// Gets or sets the reply_status.
        /// </summary>
        public string? reply_status { get; set; }

        /// <summary>
        /// Gets or sets the reply_recieved_date.
        /// </summary>
        public string? reply_recieved_date { get; set; }
    }
}