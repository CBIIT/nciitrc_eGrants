namespace eGrants.Models
{
    /// <summary>
    /// The notifications model.
    /// </summary>
    public class Notifications
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string? id { get; set; }

        /// <summary>
        /// Gets or sets the full_grant_num.
        /// </summary>
        public string? full_grant_num { get; set; }

        /// <summary>
        /// Gets or sets the appl_id.
        /// </summary>
        public string? appl_id { get; set; }

        /// <summary>
        /// Gets or sets the pa.
        /// </summary>
        public string? pa { get; set; }

        /// <summary>
        /// Gets or sets the subject line.
        /// </summary>
        public string? subjectLine { get; set; }

        /// <summary>
        /// Gets or sets the notification body.
        /// </summary>
        public string? NotificationBody { get; set; }

        /// <summary>
        /// Gets or sets the not rcvd_dt.
        /// </summary>
        public string? NotRcvd_dt { get; set; }

        /// <summary>
        /// Gets or sets the created_date.
        /// </summary>
        public string? created_date { get; set; }
    }
}