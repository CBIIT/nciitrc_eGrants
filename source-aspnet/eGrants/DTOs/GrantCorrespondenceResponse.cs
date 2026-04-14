using System.Collections.Generic;

namespace eGrants.DTOs
{
    /// <summary>
    /// Response DTO for the Grant Correspondence REST API.
    /// </summary>
    public class GrantCorrespondenceResponse
    {
        /// <summary>
        /// Gets or sets the list of correspondence data items.
        /// </summary>
        public List<CorrespondenceDataDto> CorrespondenceData { get; set; }
    }

    /// <summary>
    /// Represents a single correspondence data item from the Grant Correspondence API.
    /// </summary>
    public class CorrespondenceDataDto
    {
        /// <summary>
        /// Gets or sets the notification name.
        /// </summary>
        public string NotificationName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        public string SentDate { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the to address.
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the CC address.
        /// </summary>
        public string CcAddress { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the email content.
        /// </summary>
        public string EmailContent { get; set; }
    }
}
