using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VK_mini_app.Models
{
    public class ResponseGPT
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";
        [JsonProperty("object")]
        public string Object { get; set; } = "";
        [JsonProperty("created")]
        public ulong Created { get; set; }
        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; } = new();
        [JsonProperty("usage")]
        public Usage Usage { get; set; } = new();
    }

    public class Choice
    {
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("message")]
        public Message Message { get; set; } = new();
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; } = "";
    }

    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
