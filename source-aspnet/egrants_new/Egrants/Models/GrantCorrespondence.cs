using Newtonsoft.Json;

namespace egrants_new.Models
{
    public class GrantCorrespondence
    {
        [JsonProperty("applId")]
        public string ApplId { get; set; }

        // Your sample shows correspondenceData as an object.
        // If the API returns an array sometimes, switch this to JToken and handle both.
        [JsonProperty("correspondenceData")]
        public Correspondence CorrespondenceData { get; set; }
    }
}