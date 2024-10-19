using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VK_mini_app.Models
{
    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = "";
        [JsonProperty("content")]
        public string Content { get; set; } = "";
    }
}
