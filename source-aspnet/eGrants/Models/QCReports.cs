namespace eGrants.Models
{
    public class QCReports
    {
        public string? qc_days { get; set; }

        /// <summary>
        ///     Gets or sets the files_to_qc.
        /// </summary>
        public string? files_to_qc { get; set; }

        /// <summary>
        ///     Gets or sets the qc_person_id.
        /// </summary>
        public string? qc_person_id { get; set; }

        /// <summary>
        ///     Gets or sets the qc_person_name.
        /// </summary>
        public string? qc_person_name { get; set; }
    }
}
