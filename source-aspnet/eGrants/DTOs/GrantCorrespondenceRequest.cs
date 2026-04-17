using Newtonsoft.Json;

namespace eGrants.DTOs
{
    /// <summary>
    /// Request DTO for the Grant Correspondence REST API.
    /// Matches the camelCase JSON field names expected by the ERA service.
    /// </summary>
    public class GrantCorrespondenceRequest
    {
        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        [JsonProperty("applId")]
        public string ApplId { get; set; }
    }
}
