using Newtonsoft.Json;

namespace VK_mini_app.Models.Response
{
    public class NotificationResponseModel
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }
}
