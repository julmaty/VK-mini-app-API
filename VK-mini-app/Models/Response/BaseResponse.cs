using Azure;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VK_mini_app.Models.Response
{
    public class BaseResponse<TResponse>
    {
        [JsonProperty("response")]
        public TResponse Response { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}
