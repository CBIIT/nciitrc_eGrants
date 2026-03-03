using Newtonsoft.Json;

namespace egrants_new.Models
{
    public class GrantCorrespondenceRequest
    {
        [JsonProperty("applId")]
        public string ApplId { get; set; }
    }
}