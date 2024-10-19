using System;

namespace VK_mini_app.Models.Request
{
    public class getUsersRequest : BaseRequest
    {
        public List<string> user_ids { get; set; }
        public string name_case { get; set; }
        public int from_group_id { get; set; } 
    }
}
