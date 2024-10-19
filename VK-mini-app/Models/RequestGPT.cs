using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VK_mini_app.Models
{
    class RequestGPT
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "";
        [JsonProperty("messages")]
        public List<Message> Messages { get; set; } = new();
    }
}
