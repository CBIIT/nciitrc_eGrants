using Newtonsoft.Json;

namespace egrants_new.Models
{
    public class Correspondence
    {
        [JsonProperty("notificationName")]
        public string NotificationName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("ccAddress")]
        public string CcAddress { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("sentDate")]
        public string SentDate { get; set; }

        [JsonProperty("emailContent")]
        public string EmailContent { get; set; }
    }
}