using VK_mini_app.Models.Request;

namespace VK_mini_app.Models.Response
{
    public class GetUsersResponse
    {
        public int id {  get; set; }
        public string first_name { get; set; }
        public string last_name { get; set;}
        public bool can_access_closed { get; set; }
        public bool is_closed { get; set; }
        public string activities {  get; set; }
        public string about {  get; set; }
        public string books { get; set; }
        public string education { get; set; }
        public string sex { get; set; }
        public string games { get; set; }
        public string interests { get; set; }
        public string movies { get; set; }
        public string music { get; set; }
        public string quotes { get; set; }
    }
}
