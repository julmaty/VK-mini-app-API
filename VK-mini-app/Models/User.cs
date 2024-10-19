namespace VK_mini_app.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int VkId { get; set; }
        public int Score { get; set; }
        public string AvatarUrl { get; set; }
        public int UserInfoId { get; set; }
    }
}
