using System.Collections.Generic;
using Newtonsoft.Json;

namespace eGrants.DTOs
{
    /// <summary>
    /// Response DTO for the Grant Correspondence REST API.
    /// Uses [JsonProperty] to match the camelCase JSON field names returned by the ERA service.
    /// </summary>
    public class GrantCorrespondenceResponse
    {
        /// <summary>
        /// Gets or sets the list of correspondence data items.
        /// </summary>
        [JsonProperty("correspondenceData")]
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
        [JsonProperty("notificationName")]
        public string NotificationName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the sent date.
        /// </summary>
        [JsonProperty("sentDate")]
        public string SentDate { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the to address.
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the CC address.
        /// </summary>
        [JsonProperty("ccAddress")]
        public string CcAddress { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        [JsonProperty("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the email content.
        /// </summary>
        [JsonProperty("emailContent")]
        public string EmailContent { get; set; }
    }
}
