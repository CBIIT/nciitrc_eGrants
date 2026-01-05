namespace eGrants.Models
{

    /// <summary>
    ///     The stop notice.
    /// </summary>
    public class StopNoticeObject
    {
        /// <summary>
        ///     Gets or sets the appl_id.
        /// </summary>
        public string appl_id { get; set; }

        /// <summary>
        ///     Gets or sets the full_grant_num.
        /// </summary>
        public string full_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the closeout_fsr_code.
        /// </summary>
        public string closeout_fsr_code { get; set; }

        /// <summary>
        ///     Gets or sets the final_invention_stmnt_code.
        /// </summary>
        public string final_invention_stmnt_code { get; set; }

        /// <summary>
        ///     Gets or sets the final_report_date.
        /// </summary>
        public string final_report_date { get; set; }
    }
}